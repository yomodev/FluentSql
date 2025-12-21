
namespace FluentSqlLib;

public class FluentUdfContext(IFluentSqlClient client, string udfName) : IFluentUdfContext
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

    public T GetRequired<T>()
    {
        throw new NotImplementedException();
    }

    public T GetRequired<T>(string column)
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

    public IUdfInputParam WithParam<T>(string paramName, T value)
    {
        throw new NotImplementedException();
    }
}