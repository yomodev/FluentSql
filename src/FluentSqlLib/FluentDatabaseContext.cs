using System.Runtime.CompilerServices;

namespace FluentSqlLib;

public class FluentDatabaseContext(IFluentSql fluentSql, string databaseName)
    : IFluentDatabaseContext
{
    public string Name => databaseName;

    public ValueTask<bool> DropIndexAsync(
        string tableName, string indexName, CancellationToken cancellationToken = default)
        => ExecuteDdlAsync(builder => builder.BuildDropIndexSql(indexName, tableName), cancellationToken);

    public ValueTask<bool> DropStoredProcedureAsync(
        string procedureName, CancellationToken cancellationToken = default)
        => ExecuteDdlAsync(builder => builder.BuildDropStoredProcedureSql(procedureName), cancellationToken);

    public ValueTask<bool> DropTableAsync(
        string tableName, CancellationToken cancellationToken = default)
        => ExecuteDdlAsync(builder => builder.BuildDropTableSql(tableName), cancellationToken);

    public ValueTask<bool> DropFunctionAsync(
        string functionName, CancellationToken cancellationToken = default)
        => ExecuteDdlAsync(builder => builder.BuildDropFunctionSql(functionName), cancellationToken);

    public ValueTask<bool> DropViewAsync(
        string viewName, CancellationToken cancellationToken = default)
        => ExecuteDdlAsync(builder => builder.BuildDropViewSql(viewName), cancellationToken);

    public ValueTask<bool> TruncateTableAsync(
        string tableName, CancellationToken cancellationToken = default)
        => ExecuteDdlAsync(builder => builder.BuildTruncateTableSql(tableName), cancellationToken);

    public async IAsyncEnumerable<string> ListSchemasAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new NoResultQuery(@"
            SELECT
                SCHEMA_NAME AS Name
            FROM
                INFORMATION_SCHEMA.SCHEMATA");
        using var client = CreateScopedClient(query);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListFunctionsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new NoResultQuery(@"
            SELECT
                CONCAT(ROUTINE_SCHEMA, '.', ROUTINE_NAME) AS Name
            FROM
                INFORMATION_SCHEMA.ROUTINES
            WHERE
                ROUTINE_TYPE = 'FUNCTION'");
        using var client = CreateScopedClient(query);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListStoredProceduresAsync(
       [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new Query(@"
            SELECT
                CONCAT(ROUTINE_SCHEMA, '.', ROUTINE_NAME) AS Name
            FROM
                INFORMATION_SCHEMA.ROUTINES
            WHERE
                ROUTINE_TYPE = 'PROCEDURE'");
        using var client = CreateScopedClient(query);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListTablesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new Query(@"
            SELECT
                CONCAT(TABLE_SCHEMA, '.', TABLE_NAME) AS Name
            FROM
                INFORMATION_SCHEMA.TABLES
            WHERE
                TABLE_TYPE = 'BASE TABLE'");
        using var client = CreateScopedClient(query);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListViewsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new Query(@"
            SELECT
                CONCAT(TABLE_SCHEMA, '.', TABLE_NAME) AS Name
            FROM
                INFORMATION_SCHEMA.VIEWS");
        using var client = CreateScopedClient(query);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    private async ValueTask<bool> ExecuteDdlAsync(
        Func<ISqlClient, string> buildSql, CancellationToken cancellationToken)
    {
        using var builder = fluentSql.CreateClient(new NoResultQuery(string.Empty));
        var sqlText = buildSql(builder);
        using var client = CreateScopedClient(new NoResultQuery(sqlText));
        return await client.ExecuteAsync(cancellationToken) != 0;
    }

    private ISqlClient CreateScopedClient(IQuery query)
    {
        var client = fluentSql.CreateClient(query);
        client.TargetDatabase = Name;
        return client;
    }
}
