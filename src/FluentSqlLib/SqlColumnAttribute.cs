namespace FluentSqlLib;

/// <summary>
/// Describes how a DTO property maps to a SQL column when no explicit column mapper is supplied.
/// Applies to result mapping (<see cref="RuntimeMapper"/>, <see cref="Mapper.DataReaderMapper"/>)
/// and parameter/TVP generation (<see cref="Mapper.SqlParameterFactory"/>, <see cref="Mapper.TvpBuilder"/>).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class SqlColumnAttribute : Attribute
{
    private const int UnsetOrdinal = -1;
    private const int UnsetSize = -1;
    private const DbType UnsetDbType = (DbType)(-1);

    /// <summary>Overrides the column name; defaults to the property name.</summary>
    public string? Name { get; set; }

    /// <summary>Overrides column resolution by name with a fixed ordinal position.</summary>
    public int Ordinal { get; set; } = UnsetOrdinal;

    /// <summary>Excludes the property entirely from mapping and parameter generation.</summary>
    public bool Ignore { get; set; }

    /// <summary>Overrides the inferred <see cref="System.Data.DbType"/> for this column.</summary>
    public DbType DbType { get; set; } = UnsetDbType;

    /// <summary>Overrides the inferred parameter size.</summary>
    public int Size { get; set; } = UnsetSize;

    /// <summary>Decimal precision. A value greater than zero marks precision/scale as specified.</summary>
    public byte Precision { get; set; }

    /// <summary>Decimal scale. Only honored when <see cref="Precision"/> is greater than zero.</summary>
    public byte Scale { get; set; }

    /// <summary>Marks the column as database-computed; excluded from generated INSERT/UPDATE parameters.</summary>
    public bool Computed { get; set; }

    /// <summary>Marks the column as an identity column; excluded from generated INSERT/UPDATE parameters.</summary>
    public bool Identity { get; set; }

    public bool HasOrdinal => Ordinal != UnsetOrdinal;

    public bool HasDbType => DbType != UnsetDbType;

    public bool HasSize => Size != UnsetSize;

    public bool HasPrecisionScale => Precision > 0;
}
