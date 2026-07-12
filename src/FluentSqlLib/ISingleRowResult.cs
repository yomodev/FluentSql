namespace FluentSqlLib;

public interface ISingleRowResult
{
    ValueTask<T> GetAsync<T>(CancellationToken cancellationToken = default);

    ValueTask<T?> GetAsync<T>(string column, CancellationToken cancellationToken = default);

    ValueTask<T> GetAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default);

    ValueTask<T> GetRequiredAsync<T>(CancellationToken cancellationToken = default);

    ValueTask<T> GetRequiredAsync<T>(string column, CancellationToken cancellationToken = default);

    // --- Synchronous counterparts ---

    T Get<T>();

    T? Get<T>(string column);

    T Get<T>(string column, T defaultValue);

    T GetRequired<T>();

    T GetRequired<T>(string column);
}