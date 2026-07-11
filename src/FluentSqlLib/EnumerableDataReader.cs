namespace FluentSqlLib;

/// <summary>
/// A forward-only, single-pass <see cref="IDataReader"/> over an <see cref="IEnumerable{T}"/>,
/// for streaming rows into <see cref="System.Data.SqlClient.SqlBulkCopy"/> without ever
/// materializing a <see cref="DataTable"/>. Column values for the current row are computed
/// once per <see cref="Read"/> via <see cref="BulkRowAccessor"/>'s cached compiled accessors,
/// not recomputed per <see cref="GetValue"/>/<see cref="IsDBNull"/> call.
/// </summary>
internal sealed class EnumerableDataReader<T> : IDataReader
{
    private readonly IEnumerator<T> _source;
    private readonly IReadOnlyList<ColumnMap> _columns;
    private readonly Func<T, object?>[] _accessors;
    private object?[] _currentValues;
    private bool _disposed;

    public EnumerableDataReader(IEnumerable<T> rows)
    {
        _source = rows.GetEnumerator();
        (_columns, _accessors) = BulkRowAccessor.GetAccessors<T>();
        _currentValues = new object?[_accessors.Length];
    }

    /// <summary>Column names, in bulk-insert order, for configuring SqlBulkCopy.ColumnMappings.</summary>
    public IReadOnlyList<ColumnMap> Columns => _columns;

    /// <summary>Rows pulled from the source so far; stable once WriteToServerAsync completes.</summary>
    public long RowsRead { get; private set; }

    public int FieldCount => _columns.Count;

    public bool Read()
    {
        if (!_source.MoveNext())
        {
            return false;
        }

        var current = _source.Current;
        for (int i = 0; i < _accessors.Length; i++)
        {
            _currentValues[i] = _accessors[i](current);
        }

        RowsRead++;
        return true;
    }

    public object GetValue(int i) => _currentValues[i] ?? DBNull.Value;

    public bool IsDBNull(int i) => _currentValues[i] is null or DBNull;

    public string GetName(int i) => _columns[i].ColumnName;

    public int GetOrdinal(string name)
    {
        for (int i = 0; i < _columns.Count; i++)
        {
            if (string.Equals(_columns[i].ColumnName, name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        throw new IndexOutOfRangeException($"Column '{name}' not found.");
    }

    public Type GetFieldType(int i)
    {
        var propertyType = _columns[i].Property.PropertyType;
        // Enum values are yielded as their numeric underlying type by BulkRowAccessor, so report
        // that here too - SqlBulkCopy must never see the enum type for an int/tinyint column.
        if (RuntimeMapper.GetEnumType(propertyType) is { } enumType)
        {
            return Enum.GetUnderlyingType(enumType);
        }

        return Nullable.GetUnderlyingType(propertyType) ?? propertyType;
    }

    public string GetDataTypeName(int i) => GetFieldType(i).Name;

    public bool GetBoolean(int i) => (bool)GetValue(i);
    public byte GetByte(int i) => (byte)GetValue(i);
    public char GetChar(int i) => (char)GetValue(i);
    public DateTime GetDateTime(int i) => (DateTime)GetValue(i);
    public decimal GetDecimal(int i) => (decimal)GetValue(i);
    public double GetDouble(int i) => (double)GetValue(i);
    public float GetFloat(int i) => (float)GetValue(i);
    public Guid GetGuid(int i) => (Guid)GetValue(i);
    public short GetInt16(int i) => (short)GetValue(i);
    public int GetInt32(int i) => (int)GetValue(i);
    public long GetInt64(int i) => (long)GetValue(i);
    public string GetString(int i) => (string)GetValue(i);

    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
        => throw new NotSupportedException("Streamed byte access is not supported; use GetValue.");

    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
        => throw new NotSupportedException("Streamed char access is not supported; use GetValue.");

    public IDataReader GetData(int i) => throw new NotSupportedException();

    public int GetValues(object[] values)
    {
        var count = Math.Min(values.Length, FieldCount);
        for (int i = 0; i < count; i++)
        {
            values[i] = GetValue(i);
        }

        return count;
    }

    public DataTable? GetSchemaTable() => null;

    public int Depth => 0;

    public bool IsClosed => _disposed;

    public int RecordsAffected => -1;

    public object this[int i] => GetValue(i);

    public object this[string name] => GetValue(GetOrdinal(name));

    public bool NextResult() => false;

    public void Close() => Dispose();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _source.Dispose();
        _disposed = true;
    }
}
