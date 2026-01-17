namespace FluentSqlLib;

public class FluentStoredProcedureContext(IFluentSql fluentSql, string procedureName) : IFluentStoredProcedureContext
{
    internal ISqlClient _client = fluentSql.CreateClient(new StoredProcedureQuery(procedureName));

    public IAsyncEnumerable<T> EnumerateAsync<T>(CancellationToken cancellationToken)
        => _client.EnumerateAsync<T>(cancellationToken);

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
