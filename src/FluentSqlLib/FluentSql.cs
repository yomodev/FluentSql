using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace FluentSqlLib;

public class FluentSql<TSettings>(ILogger<TSettings> logger, TSettings settings)
    : IFluentSql
    where TSettings : IFluentSqlSettings
{
    public IFluentDatabaseContext CurrentDatabase {
        get
        {
            var builder = new SqlConnectionStringBuilder(settings.ConnectionString);
            string databaseName = builder.InitialCatalog;
            return new FluentDatabaseContext(this, databaseName);
        } 
    }

    public async IAsyncEnumerable<string> ListDatabasesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var client = CreateClient("SELECT name FROM sys.databases");
        await foreach(var record in client.EnumerateAsync(cancellationToken))
        {
            yield return record.GetString(0);
        }
    }

    public IFluentSqlClient CreateClient(string query)
        => new FluentSqlClient<TSettings>(logger, settings, query);

    public IFluentSqlClient CreateSPClient(string procedureName)
        => new FluentSpClient<TSettings>(logger, settings, procedureName);

    public IFluentSqlTransaction BeginTransaction()
    {
        return new FluentSqlTransaction(this);
    }

    public IFluentDatabaseContext Database(string name) =>
        new FluentDatabaseContext(this, name);

    public IFluentDeleteQueryContext Delete(string name) =>
        new FluentQueryContext(this, name);

    public void ExecuteTransaction(Action<IFluentSqlTransaction> action)
    {
        using var tran = BeginTransaction();
        try
        {
            action(tran);
            tran.Commit();
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public IFluentInsertQueryContext Insert(string name) =>
     new FluentQueryContext(this, name);

    public IFluentQueryContext Query(string sql) =>
        new FluentQueryContext(this, sql);
  
    public IFluentSelectQueryContext Select(string name) =>
        new FluentQueryContext(this, name);

    public IFluentStoredProcedureContext StoredProcedure(string name) =>
        new FluentStoredProcedureContext(this, name);

    public IFluentTableContext Table(string name) =>
        new FluentTableContext(this, name);

    public IFluentUdfContext Udf(string name) =>
        new FluentUdfContext(this, name);
   
    public IFluentUpdateQueryContext Update(string name) =>
        new FluentQueryContext(this, name);
}
