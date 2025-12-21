namespace FluentSqlLib.Interfaces;

public interface ISingleRowResult
{
    /*T Get<T>();

    T? Get<T>(string column);

    T Get<T>(string column, T defaultValue);

    T GetRequired<T>();

    T GetRequired<T>(string column);*/

    Task<T> GetAsync<T>(CancellationToken cancellationToken = default);

    Task<T?> GetAsync<T>(string column, CancellationToken cancellationToken = default);

    Task<T> GetAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default);

    Task<T> GetRequiredAsync<T>(CancellationToken cancellationToken = default);

    Task<T> GetRequiredAsync<T>(string column, CancellationToken cancellationToken = default);
}