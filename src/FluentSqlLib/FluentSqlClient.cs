using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace FluentSqlLib;

public class FluentSqlClient<TSettings>(
    ILogger<TSettings> logger,
    TSettings settings,
    string query)
    : IFluentSqlClient
    where TSettings : IFluentSqlSettings
{
    private bool _disposed;
    private SqlConnection? _connection;
    private SqlCommand? _command;

    public IEnumerable<IDataReader> Enumerate()
    {
        Connect();
        var behaviour = CommandBehavior.CloseConnection;
        using var reader = _command!.ExecuteReader(behaviour);
        while (reader.Read())
        {
            yield return reader;
        }
        while (reader.NextResult())
        {
        }
    }

    public async IAsyncEnumerable<IDataReader> EnumerateAsync(
        CommandBehavior behavior,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await ConnectAsync(cancellationToken);
        using var reader = await _command!.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return reader;
        }
        while (reader.NextResult())
        {
        }
    }

    public IAsyncEnumerable<IDataReader> EnumerateAsync(CancellationToken cancellationToken = default)
    {
        var behavior = CommandBehavior.SingleResult | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection;
        return EnumerateAsync(behavior, cancellationToken);
    }

    public IAsyncEnumerable<T> EnumerateAsync<T>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<T> GetAsync<T>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<T?> GetAsync<T>(string column, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<T> GetAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<IReadOnlyDictionary<string, object?>> GetOutputAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<T> GetOutputAsync<T>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<T?> GetOutputAsync<T>(string column, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<T> GetOutputAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<T> GetRequiredAsync<T>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<T> GetRequiredAsync<T>(string column, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<long> InsertManyAsync<T>(IEnumerable<T> rows, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ISpParam WithOutputParam<T>(string paramName, T value)
    {
        throw new NotImplementedException();
    }

    public ISpParam WithParam<T>(string paramName, T value)
    {
        throw new NotImplementedException();
    }

    public ISpParam WithParam<T>(string paramName, IEnumerable<T> tableValued, string tableTypeName)
    {
        throw new NotImplementedException();
    }

    internal SqlConnection Connect()
    {
        _connection ??= CreateConnection();
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        _command = _connection.CreateCommand();
        _command.CommandText = query;
        return _connection;
    }

    private SqlConnection CreateConnection()
    {
        var connection = new SqlConnection(settings.ConnectionString);
        if (logger.IsEnabled(LogLevel.Trace))
        {
            connection.StateChange += LogConnectionState;
            connection.InfoMessage += LogInfoMessage;
            connection.Disposed += (sender, args) =>
            {
                logger.LogTrace("SQL Connection disposed");
                ((SqlConnection)sender!).StateChange -= LogConnectionState;
                ((SqlConnection)sender!).InfoMessage -= LogInfoMessage;
            };
        }
        return connection;
    }

    private void LogConnectionState(object sender, StateChangeEventArgs args)
    {
        logger.LogTrace("SQL Connection state changed from {OriginalState} to {CurrentState}",
                    args.OriginalState, args.CurrentState);
    }

    private void LogInfoMessage(object sender, SqlInfoMessageEventArgs args)
    {
        foreach (SqlError error in args.Errors)
        {
            logger.LogTrace("SQL InfoMessage: Number={Number}, State={State}, Class={Class}, Server={Server}, Message={Message}",
                error.Number, error.State, error.Class, error.Server, error.Message);
        }
    }

    internal async ValueTask<SqlConnection> ConnectAsync(CancellationToken cancellation)
    {
        _connection ??= CreateConnection();
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellation);
        }

        _command = _connection.CreateCommand();
        _command.CommandText = query;
        return _connection;
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
            //_connection?.Dispose();
        }

        _disposed = true;
    }
}
