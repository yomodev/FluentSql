using System.Reflection;

namespace FluentSqlLib;

/// <summary>
/// Resolves a DTO property's effective column mapping, applying <see cref="SqlColumnAttribute"/>
/// overrides when present and falling back to convention (property name, inferred DbType) otherwise.
/// </summary>
internal sealed class ColumnMap
{
    public PropertyInfo Property { get; }

    public string ColumnName { get; }

    public int? Ordinal { get; }

    public bool Ignore { get; }

    public DbType? DbType { get; }

    public int? Size { get; }

    public byte? Precision { get; }

    public byte? Scale { get; }

    public bool Computed { get; }

    public bool Identity { get; }

    private ColumnMap(PropertyInfo property, SqlColumnAttribute? attribute)
    {
        Property = property;
        ColumnName = attribute?.Name ?? property.Name;
        Ordinal = attribute is { HasOrdinal: true } ? attribute.Ordinal : null;
        Ignore = attribute?.Ignore ?? false;
        DbType = attribute is { HasDbType: true } ? attribute.DbType : null;
        Size = attribute is { HasSize: true } ? attribute.Size : null;
        Precision = attribute is { HasPrecisionScale: true } ? attribute.Precision : null;
        Scale = attribute is { HasPrecisionScale: true } ? attribute.Scale : null;
        Computed = attribute?.Computed ?? false;
        Identity = attribute?.Identity ?? false;
    }

    public static IReadOnlyList<ColumnMap> ResolveAll(Type type)
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => new ColumnMap(p, p.GetCustomAttribute<SqlColumnAttribute>()))
            .Where(m => !m.Ignore)
            .ToArray();
}
