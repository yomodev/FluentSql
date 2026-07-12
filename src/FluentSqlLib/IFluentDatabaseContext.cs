namespace FluentSqlLib;

/// <summary>
/// Database-level metadata and DDL: list tables/views/functions/procedures/schemas, and drop or
/// truncate objects. Operations are scoped to <see cref="Name"/> by pointing the connection at that
/// database, and generate provider-appropriate SQL via the client's Build*Sql methods.
/// </summary>
public interface IFluentDatabaseContext
{
    /// <summary>The target database name.</summary>
    string Name { get; }

    ValueTask<bool> DropIndexAsync(
        string tableName, string indexName, CancellationToken cancellationToken = default);

    ValueTask<bool> DropStoredProcedureAsync(
        string procedureName, CancellationToken cancellationToken = default);

    ValueTask<bool> DropTableAsync(
        string tableName, CancellationToken cancellationToken = default);

    ValueTask<bool> DropFunctionAsync(
        string functionName, CancellationToken cancellationToken = default);

    ValueTask<bool> DropViewAsync(
        string viewName, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ListSchemasAsync(
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ListFunctionsAsync(
        CancellationToken cancellationToken = default);


    IAsyncEnumerable<string> ListStoredProceduresAsync(
       CancellationToken cancellationToken = default);


    IAsyncEnumerable<string> ListTablesAsync(
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ListViewsAsync(
        CancellationToken cancellationToken = default);

    ValueTask<bool> TruncateTableAsync(
        string tableName, CancellationToken cancellationToken = default);
}