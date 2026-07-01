using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics;

namespace FluentSqlLib;

public class SqlServerClient<TSettings>(
    ILogger<TSettings> _logger,
    TSettings _settings,
    IQuery query)
    : ClientBase<TSettings>(_logger, _settings, query)
    where TSettings : IFluentSqlSettings
{
    public override DbConnection CreateConnection()
    {
        var connectionString = settings.ConnectionString;
        if (!string.IsNullOrEmpty(TargetDatabase))
        {
            var builder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = TargetDatabase };
            connectionString = builder.ConnectionString;
        }

        var connection = new SqlConnection(connectionString);
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

    public override DbParameter CreateParameter(DbCommand command, QueryParameter qParam)
    {
        var sqlParam = base.CreateParameter(command, qParam) as SqlParameter;
        Debug.Assert(sqlParam is not null);
        if (qParam.TableTypeName.Length > 0)
        {
            if (qParam is not IGenericQueryParameter genericParam)
            {
                throw new InvalidOperationException(
                    $"Table-valued parameter '{qParam.Name}' must be created via QueryParameter<T> so its row type is known.");
            }

            var rows = qParam.Value as System.Collections.IEnumerable
                ?? throw new InvalidOperationException($"Table-valued parameter '{qParam.Name}' has no rows.");

            sqlParam.Value = Mapper.TvpBuilder.BuildDataTable(rows, genericParam.Type);
            sqlParam.SqlDbType = SqlDbType.Structured;
            sqlParam.TypeName = qParam.TableTypeName;
        }
        return sqlParam;
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
