namespace FluentSqlLib;

public interface IFluentSelectQueryContext
{
    IFluentSelectQueryContext WithParam<T>(string name, T value);

    IAsyncEnumerable<T> QueryAsync<T>(
        bool skipMissingColumns = true, CancellationToken cancellationToken = default) where T : new();

    IAsyncEnumerable<T> QueryAsync<T>(
        IReadOnlyDictionary<string, string> propertyToColumn,
        bool skipMissingColumns = true, CancellationToken cancellationToken = default) where T : new();

    IAsyncEnumerable<T> QueryAsync<T>(
        IReadOnlyDictionary<string, Action<T, IDataRecord>> columnSetters,
        bool skipMissingColumns = true, CancellationToken cancellationToken = default) where T : new();

    IAsyncEnumerable<T> QueryAsync<T>(
        Func<IDataRecord, T> mapper, CancellationToken cancellationToken = default);

    ValueTask<T?> GetAsync<T>(string column, CancellationToken cancellationToken = default);

    ValueTask<T> GetAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default);
}