using System.Runtime.CompilerServices;

namespace FluentSqlLib;

public class FluentDatabaseContext(IFluentSql fluentSql, string databaseName)
    : IFluentDatabaseContext
{
    public string Name => databaseName;

    public async ValueTask<bool> DropIndexAsync(
        string indexName, CancellationToken cancellationToken = default)
    {
        // TODO make generic for other providers and verify existence before dropping
        var sql = new NoResultQuery($@"DROP INDEX {indexName} ON [{Name}]");
        using var client = fluentSql.CreateClient(sql);
        return await client.ExecuteAsync(cancellationToken) != 0;
    }

    public async ValueTask<bool> DropStoredProcedureAsync(
        string procedureName, CancellationToken cancellationToken = default)
    {
        // TODO make generic for other providers and verify existence before dropping
        var sql = new NoResultQuery($@"DROP PROCEDURE {procedureName} ON [{Name}]");
        using var client = fluentSql.CreateClient(sql);
        return await client.ExecuteAsync(cancellationToken) != 0;
    }

    public async ValueTask<bool> DropTableAsync(
        string tableName, CancellationToken cancellationToken = default)
    {
        // TODO make generic for other providers and verify existence before dropping
        var sql = new NoResultQuery($@"DROP TABLE {tableName} ON [{Name}]");
        using var client = fluentSql.CreateClient(sql);
        return await client.ExecuteAsync(cancellationToken) != 0;
    }

    public async ValueTask<bool> DropFunctionAsync(
        string functionName, CancellationToken cancellationToken = default)
    {
        // TODO make generic for other providers and verify existence before dropping
        var sql = new NoResultQuery($@"DROP FUNCTION {functionName} ON [{Name}]");
        using var client = fluentSql.CreateClient(sql);
        return await client.ExecuteAsync(cancellationToken) != 0;
    }

    public async ValueTask<bool> DropViewAsync(
        string name, CancellationToken cancellationToken = default)
    {
        // TODO make generic for other providers and verify existence before dropping
        var sql = new NoResultQuery($@"DROP VIEW {name} ON [{Name}]");
        using var client = fluentSql.CreateClient(sql);
        return await client.ExecuteAsync(cancellationToken) != 0;
    }

    public async IAsyncEnumerable<string> ListSchemasAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new NoResultQuery($@"
            SELECT 
                SCHEMA_NAME AS Name
            FROM 
                [{Name}].INFORMATION_SCHEMA.SCHEMATA");
        using var client = fluentSql.CreateClient(query);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListFunctionsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new NoResultQuery($@"
            SELECT 
                CONCAT(ROUTINE_SCHEMA, '.', ROUTINE_NAME) AS Name
            FROM 
                [{Name}].INFORMATION_SCHEMA.ROUTINES
            WHERE 
                ROUTINE_TYPE = 'FUNCTION'");
        using var client = fluentSql.CreateClient(query);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListStoredProceduresAsync(
       [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new Query($@"
            SELECT 
                CONCAT(ROUTINE_SCHEMA, '.', ROUTINE_NAME) AS Name
            FROM 
                [{Name}].INFORMATION_SCHEMA.ROUTINES
            WHERE 
                ROUTINE_TYPE = 'PROCEDURE'");
        using var client = fluentSql.CreateClient(query);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListTablesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new Query($@"
            SELECT 
                CONCAT(TABLE_SCHEMA, '.', TABLE_NAME) AS Name
            FROM 
                [{Name}].INFORMATION_SCHEMA.TABLES
            WHERE
                TABLE_TYPE = 'BASE TABLE'");
        using var client = fluentSql.CreateClient(query);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListViewsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new Query($@"
            SELECT 
                CONCAT(TABLE_SCHEMA, '.', TABLE_NAME) AS Name
            FROM 
                [{Name}].INFORMATION_SCHEMA.VIEWS");
        using var client = fluentSql.CreateClient(query);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async ValueTask<bool> TruncateTableAsync(
        string tableName, CancellationToken cancellationToken = default)
    {
        // TODO make generic for other providers and verify existence before dropping
        var sql = new NoResultQuery($@"TRUNCATE TABLE {tableName} ON [{Name}]");
        using var client = fluentSql.CreateClient(sql);
        return await client.ExecuteAsync(cancellationToken) != 0;
    }
}
