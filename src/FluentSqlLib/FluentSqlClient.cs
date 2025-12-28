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

    public async IAsyncEnumerable<IDataReader> EnumerateAsync(
        CommandBehavior behavior,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Connect(cancellationToken);
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

    internal async ValueTask<SqlConnection> Connect(CancellationToken cancellation)
    {
        _connection ??= new SqlConnection(settings.ConnectionString);
        if (_connection.State != System.Data.ConnectionState.Open)
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
