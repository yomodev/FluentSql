using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace FluentSqlLib;

public class ClientBase<TSettings>(
    ILogger<TSettings> logger,
    TSettings settings,
    IQuery query)
    : ISqlClient, ISqlParam
    where TSettings : IFluentSqlSettings
{
    private bool _disposed;
    protected readonly ILogger<TSettings> logger = logger;
    protected readonly TSettings settings = settings;
    protected readonly IQuery query = query;
    protected readonly List<QueryParameter> parameters = [];

    internal virtual DbConnection? Connection { get; set; }

    internal virtual DbCommand? Command { get; set; }

    public virtual IEnumerable<IDataReader> Enumerate()
    {
        Connect();
        var behaviour = CommandBehavior.CloseConnection;
        using var reader = Command!.ExecuteReader(behaviour);
        while (reader.Read())
        {
            yield return reader;
        }
        while (reader.NextResult())
        {
        }
    }

    public virtual async IAsyncEnumerable<IDataReader> EnumerateAsync(
        CommandBehavior behavior,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await ConnectAsync(cancellationToken);
        using var reader = await Command!.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return reader;
        }
        while (reader.NextResult())
        {
        }
    }

    public virtual IAsyncEnumerable<IDataReader> EnumerateAsync(CancellationToken cancellationToken = default)
    {
        var behavior = CommandBehavior.SingleResult | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection;
        return EnumerateAsync(behavior, cancellationToken);
    }

    public virtual IAsyncEnumerable<T> EnumerateAsync<T>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<int> ExecuteAsync()
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<T> GetAsync<T>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<T?> GetAsync<T>(string column, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<T> GetAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<IReadOnlyDictionary<string, object?>> GetOutputAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<T> GetOutputAsync<T>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<T?> GetOutputAsync<T>(string column, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<T> GetOutputAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public T GetRequired<T>()
    {
        throw new NotImplementedException();
    }

    public T GetRequired<T>(string column)
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<T> GetRequiredAsync<T>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<T> GetRequiredAsync<T>(string column, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual ValueTask<long> InsertManyAsync<T>(IEnumerable<T> rows, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual ISqlParam WithOutputParam<T>(string name, T value)
    {
        throw new NotImplementedException();
    }

    public virtual ISqlParam WithParam<T>(string name, T value)
    {
        parameters.Add(new QueryParameter<T>(name, value));
        return this;
    }

    public virtual ISqlParam WithParam<T>(string name, IEnumerable<T> tableValued, string tableTypeName)
    {
        parameters.Add(new QueryParameter<T>
        (
            name,
            tableValued,
            tableTypeName
        ));

        return this;
    }

    internal virtual DbConnection Connect()
    {
        ArgumentNullException.ThrowIfNull(Connection);
        if (Connection.State != ConnectionState.Open)
        {
            Connection.Open();
        }

        Command = Connection.CreateCommand();
        Command.CommandText = query.ToString();
        return Connection;
    }

    internal virtual async ValueTask<DbConnection> ConnectAsync(CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(Connection);
        if (Connection.State != ConnectionState.Open)
        {
            await Connection.OpenAsync(cancellation);
        }

        Command = Connection.CreateCommand();
        Command.CommandText = query.ToString();
        return Connection;
    }

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
            parameters.Clear();
            parameters.TrimExcess();
            //_connection?.Dispose();
        }

        _disposed = true;
    }
}
