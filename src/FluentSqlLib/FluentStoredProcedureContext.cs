namespace FluentSqlLib;

public class FluentStoredProcedureContext(IFluentSql fluentSql, string procedureName) : IFluentStoredProcedureContext
{
    internal ISqlClient _client = fluentSql.CreateClient(new StoredProcedureQuery(procedureName));

    public IAsyncEnumerable<T> EnumerateAsync<T>(
        bool skipMissingColumns = true, CancellationToken cancellationToken = default) where T : new()
        => _client.EnumerateAsync<T>(skipMissingColumns, cancellationToken);

    public IAsyncEnumerable<T> EnumerateAsync<T>(
        IReadOnlyDictionary<string, string> propertyToColumn,
        bool skipMissingColumns = true, CancellationToken cancellationToken = default) where T : new()
        => _client.EnumerateAsync<T>(propertyToColumn, skipMissingColumns, cancellationToken);

    public IAsyncEnumerable<T> EnumerateAsync<T>(
        IReadOnlyDictionary<string, Action<T, IDataRecord>> columnSetters,
        bool skipMissingColumns = true, CancellationToken cancellationToken = default) where T : new()
        => _client.EnumerateAsync<T>(columnSetters, skipMissingColumns, cancellationToken);

    public IAsyncEnumerable<T> EnumerateAsync<T>(
        Func<IDataRecord, T> mapper, CancellationToken cancellationToken = default)
        => _client.EnumerateAsync<T>(mapper, cancellationToken);

    public ValueTask<IMultipleResultReader> QueryMultipleAsync(CancellationToken cancellationToken = default)
        => _client.QueryMultipleAsync(cancellationToken);

    // --- Synchronous counterparts ---

    public IEnumerable<T> Enumerate<T>(bool skipMissingColumns = true) where T : new()
        => _client.Enumerate<T>(skipMissingColumns);

    public IEnumerable<T> Enumerate<T>(
        IReadOnlyDictionary<string, string> propertyToColumn, bool skipMissingColumns = true) where T : new()
        => _client.Enumerate<T>(propertyToColumn, skipMissingColumns);

    public IEnumerable<T> Enumerate<T>(
        IReadOnlyDictionary<string, Action<T, IDataRecord>> columnSetters, bool skipMissingColumns = true) where T : new()
        => _client.Enumerate<T>(columnSetters, skipMissingColumns);

    public IEnumerable<T> Enumerate<T>(Func<IDataRecord, T> mapper)
        => _client.Enumerate<T>(mapper);

    public IMultipleResultReader QueryMultiple()
        => _client.QueryMultiple();

    public T Get<T>() => _client.Get<T>();

    public T? Get<T>(string column) => _client.Get<T>(column);

    public T Get<T>(string column, T defaultValue) => _client.Get<T>(column, defaultValue);

    public T GetRequired<T>() => _client.GetRequired<T>();

    public T GetRequired<T>(string column) => _client.GetRequired<T>(column);

    public IReadOnlyDictionary<string, object?> GetOutput() => _client.GetOutput();

    public T GetOutput<T>() => _client.GetOutput<T>();

    public T? GetOutput<T>(string column) => _client.GetOutput<T>(column);

    public T GetOutput<T>(string column, T defaultValue) => _client.GetOutput<T>(column, defaultValue);

    public ValueTask<T> GetAsync<T>(CancellationToken cancellationToken = default)
        => _client.GetAsync<T>(cancellationToken);

    public ValueTask<T?> GetAsync<T>(string column, CancellationToken cancellationToken = default)
        => _client.GetAsync<T>(column, cancellationToken);

    public ValueTask<T> GetAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default)
        => _client.GetAsync<T>(column, defaultValue, cancellationToken);

    public ValueTask<IReadOnlyDictionary<string, object?>> GetOutputAsync(CancellationToken cancellationToken = default)
        => _client.GetOutputAsync(cancellationToken);

    public ValueTask<T> GetOutputAsync<T>(CancellationToken cancellationToken = default)
        => _client.GetOutputAsync<T>(cancellationToken);

    public ValueTask<T?> GetOutputAsync<T>(string column, CancellationToken cancellationToken = default)
        => _client.GetOutputAsync<T>(column, cancellationToken);

    public ValueTask<T> GetOutputAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default)
        => _client.GetOutputAsync<T>(column, defaultValue, cancellationToken);

    public ValueTask<T> GetRequiredAsync<T>(CancellationToken cancellationToken = default)
        => _client.GetRequiredAsync<T>(cancellationToken);

    public ValueTask<T> GetRequiredAsync<T>(string column, CancellationToken cancellationToken = default)
        => _client.GetRequiredAsync<T>(column, cancellationToken);

    public ISpParam WithOutputParam<T>(string name)
    {
        _client.WithOutputParam<T>(name);
        return this;
    }

    public ISpParam WithOutputParam<T>(string name, byte precision, byte scale)
    {
        _client.WithOutputParam<T>(name, precision, scale);
        return this;
    }

    public ISpParam WithOutputParam<T>(string name, int size)
    {
        _client.WithOutputParam<T>(name, size);
        return this;
    }

    public ISpParam WithParam<T>(string name, T value)
    {
        _client.WithParam(name, value);
        return this;
    }

    public ISpParam WithParam<T>(string name, IEnumerable<T> tableValued, string tableTypeName)
    {
        _client.WithParam(name, tableValued, tableTypeName);
        return this;
    }
}
