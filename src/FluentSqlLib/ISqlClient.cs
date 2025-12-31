namespace FluentSqlLib;

public interface ISqlClient : IDisposable
{
    IAsyncEnumerable<IDataReader> EnumerateAsync(
        CancellationToken cancellationToken = default);
   
    IAsyncEnumerable<T> EnumerateAsync<T>(
        CancellationToken cancellationToken = default);

    IEnumerable<IDataReader> Enumerate();

    ValueTask<int> ExecuteAsync();

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

    ISqlParam WithOutputParam<T>(string paramName, T value);

    ISqlParam WithParam<T>(string paramName, T value);

    ISqlParam WithParam<T>(
        string paramName,
        IEnumerable<T> tableValued,
        string tableTypeName);
}
