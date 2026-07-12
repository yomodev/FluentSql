namespace FluentSqlLib;

/// <summary>
/// A SELECT (or arbitrary read) query context. Add parameters with <see cref="WithParam"/>, then
/// map rows to a DTO with <c>QueryAsync&lt;T&gt;</c>/<c>Query&lt;T&gt;</c>, read a single value with
/// <c>GetAsync&lt;T&gt;</c>/<c>Get&lt;T&gt;</c>, or read several result sets with
/// <c>QueryMultipleAsync</c>/<c>QueryMultiple</c>.
/// </summary>
/// <remarks>
/// <para><c>skipMissingColumns</c> (default true) leaves DTO properties without a matching column
/// unset; pass false to throw when a mapped column is absent.</para>
/// <para>Mapping strategies: convention/<see cref="SqlColumnAttribute"/> (default), a
/// property→column dictionary (full replacement), or a custom <c>Func&lt;IDataRecord,T&gt;</c>.</para>
/// </remarks>
public interface IFluentSelectQueryContext
{
    /// <summary>Adds a named parameter (e.g. <c>"@id"</c>) to the query.</summary>
    IFluentSelectQueryContext WithParam<T>(string name, T value);

    /// <summary>Streams rows mapped to <typeparamref name="T"/> by convention/attribute.</summary>
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