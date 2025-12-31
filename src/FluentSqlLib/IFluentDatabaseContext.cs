namespace FluentSqlLib;

public interface IFluentDatabaseContext
{
    string Name { get; }

    ValueTask<bool> DropIndexAsync(
        string indexName, CancellationToken cancellationToken = default);

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