using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace FluentSqlLib;

/// <summary>
/// Compiles and caches boxing-minimizing accessors from a DTO's properties to column values,
/// for use by <see cref="EnumerableDataReader{T}"/> during bulk insert. Mirrors
/// <see cref="RuntimeMapper"/>'s read-side pattern in the opposite direction: build the
/// Expression tree once per (type, column shape), compile it to a delegate, and cache it -
/// every subsequent row is a plain delegate call, no reflection.
/// </summary>
internal static class BulkRowAccessor
{
    private static readonly ConcurrentDictionary<string, (IReadOnlyList<ColumnMap> Columns, object Accessors)> Cache = new();

    /// <summary>
    /// Resolves the columns to bulk-insert for <typeparamref name="T"/> (excluding
    /// <see cref="SqlColumnAttribute.Computed"/> and <see cref="SqlColumnAttribute.Identity"/>
    /// properties, which the database populates itself) and a compiled accessor per column.
    /// </summary>
    public static (IReadOnlyList<ColumnMap> Columns, Func<T, object?>[] Accessors) GetAccessors<T>()
    {
        var key = typeof(T).FullName!;
        var (columns, accessors) = Cache.GetOrAdd(key, _ => Build<T>());
        return (columns, (Func<T, object?>[])accessors);
    }

    private static (IReadOnlyList<ColumnMap>, object) Build<T>()
    {
        var columns = ColumnMap.ResolveAll(typeof(T))
            .Where(c => !c.Computed && !c.Identity)
            .OrderBy(c => c.Ordinal ?? int.MaxValue)
            .ToArray();

        var accessors = new Func<T, object?>[columns.Length];
        var instance = Expression.Parameter(typeof(T), "t");

        for (int i = 0; i < columns.Length; i++)
        {
            var property = Expression.Property(instance, columns[i].Property);
            var boxedOrDbNull = BuildBoxedOrDbNullExpression(property, columns[i]);
            accessors[i] = Expression.Lambda<Func<T, object?>>(boxedOrDbNull, instance).Compile();
        }

        return (columns, accessors);
    }

    private static Expression BuildBoxedOrDbNullExpression(Expression propertyAccess, ColumnMap column)
    {
        Expression value = propertyAccess;

        if (ColumnConverterRegistry.TryGet(column.Property.PropertyType, out var converter))
        {
            value = Expression.Invoke(Expression.Constant(converter!.ToDb), propertyAccess);
        }

        var underlyingNullable = Nullable.GetUnderlyingType(value.Type);
        if (underlyingNullable is not null)
        {
            // Boxing a null Nullable<T> already yields a CLR null (not a boxed empty struct);
            // just translate that into DBNull.Value for ADO.NET's convention.
            var asObject = Expression.Convert(value, typeof(object));
            var isNull = Expression.Equal(asObject, Expression.Constant(null, typeof(object)));
            return Expression.Condition(isNull, Expression.Constant(DBNull.Value, typeof(object)), asObject);
        }

        if (!value.Type.IsValueType)
        {
            var isNull = Expression.Equal(value, Expression.Constant(null, value.Type));
            return Expression.Condition(
                isNull, Expression.Constant(DBNull.Value, typeof(object)), Expression.Convert(value, typeof(object)));
        }

        return Expression.Convert(value, typeof(object));
    }
}
