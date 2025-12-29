namespace FluentSqlLib;

public interface IFluentDatabaseContext
{
    string Name { get; }

    ValueTask<bool> DropIndexAsync(
        string name, CancellationToken cancellationToken = default);

    ValueTask<bool> DropStoredProcedureAsync(
        string name, CancellationToken cancellationToken = default);

    ValueTask<bool> DropTableAsync(
        string name, CancellationToken cancellationToken = default);

    ValueTask<bool> DropUdfAsync(
        string name, CancellationToken cancellationToken = default);

    ValueTask<bool> DropViewAsync(
        string name, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ListSchemasAsync(
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ListUdfsAsync(
        CancellationToken cancellationToken = default);


    IAsyncEnumerable<string> ListStoredProceduresAsync(
       CancellationToken cancellationToken = default);


    IAsyncEnumerable<string> ListTablesAsync(
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ListViewsAsync(
        CancellationToken cancellationToken = default);
}