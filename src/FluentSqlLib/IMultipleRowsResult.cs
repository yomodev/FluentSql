namespace FluentSqlLib;

public interface IMultipleRowsResult
{
    IAsyncEnumerable<T> EnumerateAsync<T>(
        bool skipMissingColumns = true,
        CancellationToken cancellationToken = default) where T : new();

    IAsyncEnumerable<T> EnumerateAsync<T>(
        IReadOnlyDictionary<string, string> propertyToColumn,
        bool skipMissingColumns = true,
        CancellationToken cancellationToken = default) where T : new();

    IAsyncEnumerable<T> EnumerateAsync<T>(
        IReadOnlyDictionary<string, Action<T, IDataRecord>> columnSetters,
        bool skipMissingColumns = true,
        CancellationToken cancellationToken = default) where T : new();

    IAsyncEnumerable<T> EnumerateAsync<T>(
        Func<IDataRecord, T> mapper,
        CancellationToken cancellationToken = default);
}
