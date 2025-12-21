
namespace FluentSqlLib;

public class FluentStoredProcedureContext(IFluentSqlClient client, string procedureName) : IFluentStoredProcedureContext
{
    public IAsyncEnumerable<T> EnumerateAsync<T>(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetAsync<T>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetAsync<T>(string column, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyDictionary<string, object?>> GetOutputAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetOutputAsync<T>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetOutputAsync<T>(string column, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetOutputAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetRequiredAsync<T>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetRequiredAsync<T>(string column, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ISpParam WithOutputParam<T>(string paramName, T value)
    {
        throw new NotImplementedException();
    }

    public ISpParam WithParam<T>(string paramName, T value)
    {
        throw new NotImplementedException();
    }

    public ISpParam WithParam<T>(string paramName, IEnumerable<T> tableValued, string tableTypeName)
    {
        throw new NotImplementedException();
    }
}
