using System.Collections.Concurrent;

namespace FluentSqlLib;

internal interface IColumnConverter
{
    Type PropertyType { get; }
    Type DbType { get; }
    Delegate ToDb { get; }
    Delegate FromDb { get; }
}

internal sealed class ColumnConverter<TProp, TDb> : IColumnConverter
{
    public Type PropertyType => typeof(TProp);
    public Type DbType => typeof(TDb);
    public Delegate ToDb { get; }
    public Delegate FromDb { get; }

    public ColumnConverter(Func<TProp, TDb> toDb, Func<TDb, TProp> fromDb)
    {
        ToDb = toDb;
        FromDb = fromDb;
    }
}

/// <summary>
/// Registers bidirectional conversions between a DTO property's CLR type and the CLR type
/// ADO.NET reads/writes for that column (e.g. an enum stored as a string, a value-object
/// wrapping a primitive). Consulted by <see cref="RuntimeMapper"/> on the read side and
/// <see cref="BulkRowAccessor"/> on the write side so custom types don't fall back to
/// reflection - both build the conversion straight into the compiled Expression tree.
/// </summary>
public static class ColumnConverterRegistry
{
    private static readonly ConcurrentDictionary<Type, IColumnConverter> Converters = new();

    public static void Register<TProp, TDb>(Func<TProp, TDb> toDb, Func<TDb, TProp> fromDb)
    {
        ArgumentNullException.ThrowIfNull(toDb);
        ArgumentNullException.ThrowIfNull(fromDb);
        Converters[typeof(TProp)] = new ColumnConverter<TProp, TDb>(toDb, fromDb);
    }

    public static bool Unregister<TProp>() => Converters.TryRemove(typeof(TProp), out _);

    internal static bool TryGet(Type propertyType, out IColumnConverter? converter)
        => Converters.TryGetValue(propertyType, out converter);
}
