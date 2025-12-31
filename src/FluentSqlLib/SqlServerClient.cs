using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace FluentSqlLib;

public class SqlServerClient<TSettings>(
    ILogger<TSettings> _logger,
    TSettings _settings,
    IQuery query)
    : ClientBase<TSettings>(_logger, _settings, query)
    where TSettings : IFluentSqlSettings
{
    private SqlConnection? _connection;

    internal override DbConnection? Connection
    {
        get => _connection ??= CreateConnection();
    }

    // TODO

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

}
