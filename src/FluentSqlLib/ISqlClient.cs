using System.Data.Common;

namespace FluentSqlLib;

public interface ISqlClient : IDisposable
{
    /// <summary>
    /// When set, the client connects to this database instead of the connection string's default.
    /// Providers without cross-database connections (e.g. PostgreSQL, SQLite) may ignore this or throw.
    /// </summary>
    string? TargetDatabase { get; set; }

    DbConnection CreateConnection();

    DbCommand CreateCommand(DbConnection connection);

    DbParameter CreateParameter(DbCommand command, QueryParameter qParam);

    string QuoteIdentifier(string identifier);

    string BuildDropTableSql(string tableName);

    string BuildDropIndexSql(string indexName, string tableName);

    string BuildDropStoredProcedureSql(string procedureName);

    string BuildDropFunctionSql(string functionName);

    string BuildDropViewSql(string viewName);

    string BuildTruncateTableSql(string tableName);

    IAsyncEnumerable<IDataReader> EnumerateAsync(
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<T> EnumerateAsync<T>(
        CancellationToken cancellationToken = default) where T : new();

    IEnumerable<IDataReader> Enumerate();

    int Execute();
    
    ValueTask<int> ExecuteAsync(
            CancellationToken cancellationToken = default);

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

    T GetRequired<T>();

    T GetRequired<T>(string column);

    ValueTask<T> GetRequiredAsync<T>(
        CancellationToken cancellationToken = default);

    ValueTask<T> GetRequiredAsync<T>(
        string column,
        CancellationToken cancellationToken = default);

    ValueTask<long> InsertManyAsync<T>(
        IEnumerable<T> rows,
        CancellationToken cancellationToken = default);

    ISqlParam WithOutputParam<T>(string name);

    ISqlParam WithOutputParam<T>(string name, byte precision, byte scale);

    ISqlParam WithParam<T>(string name, T value);

    ISqlParam WithParam<T>(
        string name,
        IEnumerable<T> tableValued,
        string tableTypeName);
}
