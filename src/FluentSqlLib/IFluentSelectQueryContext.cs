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

    ValueTask<IMultipleResultReader> QueryMultipleAsync(CancellationToken cancellationToken = default);

    ValueTask<T?> GetAsync<T>(string column, CancellationToken cancellationToken = default);

    ValueTask<T> GetAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default);

    // --- Synchronous counterparts ---

    IEnumerable<T> Query<T>(bool skipMissingColumns = true) where T : new();

    IEnumerable<T> Query<T>(IReadOnlyDictionary<string, string> propertyToColumn, bool skipMissingColumns = true) where T : new();

    IEnumerable<T> Query<T>(Func<IDataRecord, T> mapper);

    IMultipleResultReader QueryMultiple();

    T? Get<T>(string column);

    T Get<T>(string column, T defaultValue);
}