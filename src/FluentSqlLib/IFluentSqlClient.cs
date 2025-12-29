namespace FluentSqlLib;

public interface IFluentSqlClient : IDisposable
{
    IAsyncEnumerable<IDataReader> EnumerateAsync(
        CommandBehavior behavior,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IDataReader> EnumerateAsync(
        CancellationToken cancellationToken = default);
    IAsyncEnumerable<T> EnumerateAsync<T>(
        CancellationToken cancellationToken = default);

    IEnumerable<IDataReader> Enumerate();

    ValueTask<T> GetAsync<T>(
        CancellationToken cancellationToken = default);

    ValueTask<T?> GetAsync<T>(
        string column,
        CancellationToken cancellationToken = default);

    ValueTask<T> GetAsync<T>(
        string column,
        T defaultValue,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyDictionary<string, object?>> GetOutputAsync(
        CancellationToken cancellationToken = default);

    ValueTask<T> GetOutputAsync<T>(
        CancellationToken cancellationToken = default);

    ValueTask<T?> GetOutputAsync<T>(
        string column,
        CancellationToken cancellationToken = default);

    ValueTask<T> GetOutputAsync<T>(
        string column,
        T defaultValue,
        CancellationToken cancellationToken = default);

    ValueTask<T> GetRequiredAsync<T>(
        CancellationToken cancellationToken = default);

    ValueTask<T> GetRequiredAsync<T>(
        string column,
        CancellationToken cancellationToken = default);

    ValueTask<long> InsertManyAsync<T>(
        IEnumerable<T> rows, 
        CancellationToken cancellationToken = default);

    ISpParam WithOutputParam<T>(string paramName, T value);

    ISpParam WithParam<T>(string paramName, T value);

    ISpParam WithParam<T>(
        string paramName,
        IEnumerable<T> tableValued,
        string tableTypeName);
}
