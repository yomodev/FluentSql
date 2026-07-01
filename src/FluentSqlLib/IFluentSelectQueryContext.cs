namespace FluentSqlLib;

public interface IFluentSelectQueryContext
{
    IFluentSelectQueryContext WithParam<T>(string name, T value);

    IAsyncEnumerable<T> QueryAsync<T>(CancellationToken cancellationToken = default) where T : new();

    ValueTask<T?> GetAsync<T>(string column, CancellationToken cancellationToken = default);

    ValueTask<T> GetAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default);
}