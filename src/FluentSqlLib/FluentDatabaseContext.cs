using System.Runtime.CompilerServices;

namespace FluentSqlLib;

public class FluentDatabaseContext(IFluentSql fluentSql, string databaseName)
    : IFluentDatabaseContext
{
    public string Name => databaseName;

    public ValueTask<bool> DropIndexAsync(string name, CancellationToken cancellationToken = default)
    {
        string sql = $@"DROP INDEX {name} ON [{Name}]";
        //using var client = fluentSql.CreateDdlClient(sql);
        throw new NotImplementedException();
    }

    public ValueTask<bool> DropStoredProcedureAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> DropTableAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> DropUdfAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> DropViewAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<string> ListSchemasAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string sql = $@"
            SELECT 
                SCHEMA_NAME AS Name
            FROM 
                [{Name}].INFORMATION_SCHEMA.SCHEMATA";
        using var client = fluentSql.CreateClient(sql);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListUdfsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string sql = $@"
            SELECT 
                CONCAT(ROUTINE_SCHEMA, '.', ROUTINE_NAME) AS Name
            FROM 
                [{Name}].INFORMATION_SCHEMA.ROUTINES
            WHERE 
                ROUTINE_TYPE = 'FUNCTION'";
        using var client = fluentSql.CreateClient(sql);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListStoredProceduresAsync(
       [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string sql = $@"
            SELECT 
                CONCAT(ROUTINE_SCHEMA, '.', ROUTINE_NAME) AS Name
            FROM 
                [{Name}].INFORMATION_SCHEMA.ROUTINES
            WHERE 
                ROUTINE_TYPE = 'PROCEDURE'";
        using var client = fluentSql.CreateClient(sql);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListTablesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string sql = $@"
            SELECT 
                CONCAT(TABLE_SCHEMA, '.', TABLE_NAME) AS Name
            FROM 
                [{Name}].INFORMATION_SCHEMA.TABLES
            WHERE
                TABLE_TYPE = 'BASE TABLE'";
        using var client = fluentSql.CreateClient(sql);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public async IAsyncEnumerable<string> ListViewsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string sql = $@"
            SELECT 
                CONCAT(TABLE_SCHEMA, '.', TABLE_NAME) AS Name
            FROM 
                [{Name}].INFORMATION_SCHEMA.VIEWS";
        using var client = fluentSql.CreateClient(sql);
        await foreach (var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    // Add database operations here (Create, Drop, Exists, ListTables, etc.)
}
