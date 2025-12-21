using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace FluentSqlLib;

public class FluentSqlClient<TSettings>(ILogger<TSettings> logger, TSettings settings)
    : IFluentSqlClient, IDisposable
    where TSettings : IFluentSqlSettings
{
    private bool _disposed;
    //private SqlConnection? _connection;

    public IFluentDatabaseContext CurrentDatabase => throw new NotImplementedException();

    public async IAsyncEnumerable<string> ListDatabasesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        /*var connection = await adoClient.OpenConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sys.databases";
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return reader.GetString(0);
        }*/
        yield break;
    }
    /*
    internal async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        _connection ??= new SqlConnection(settings.ConnectionString);
        
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }

        return _connection;
    }*/

    public IFluentSqlTransaction BeginTransaction()
    {
        return new FluentSqlTransaction(this);
    }

    public IFluentDatabaseContext Database(string name)=>
        new FluentDatabaseContext(this, name);

    public void ExecuteTransaction(Action<IFluentSqlTransaction> action)
    {
        var tran = BeginTransaction();
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
        finally
        {
            tran.Dispose();
        }
    }
    public IFluentQueryContext Query(string sql)=>
        new FluentQueryContext(this, sql);

    public IFluentStoredProcedureContext StoredProcedure(string name) =>
        new FluentStoredProcedureContext(this, name);

    public IFluentTableContext Table(string name)=> 
        new FluentTableContext(this, name);

    public IFluentUdfContext Udf(string name) => 
        new FluentUdfContext(this, name);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            //_connection?.Dispose();
        }

        _disposed = true;
    }

    public IFluentDeleteQueryContext Delete(string name)
    {
        throw new NotImplementedException();
    }

    public IFluentInsertQueryContext Insert(string name)
    {
        throw new NotImplementedException();
    }

    public IFluentSelectQueryContext Select(string name)
    {
        throw new NotImplementedException();
    }

    public IFluentUpdateQueryContext Update(string name)
    {
        throw new NotImplementedException();
    }
}
