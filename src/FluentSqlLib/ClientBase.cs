using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace FluentSqlLib;

/// <summary>
/// Base implementation shared by all provider clients. Split across several partial files by
/// concern: this file holds construction, connection/command building, disposal, and the
/// stored-procedure output plumbing; see ClientBase.Ddl / .Mapping / .Execute / .Scalars /
/// .Output / .Params / .BulkInsert for the rest.
/// </summary>
public abstract partial class ClientBase<TSettings>(
    ILogger<TSettings> logger,
    TSettings settings,
    IQuery query)
    : ISqlClient, ISqlParam
    where TSettings : IFluentSqlSettings
{
    private bool _disposed;
    public string? TargetDatabase { get; set; }
    protected readonly ILogger<TSettings> logger = logger;
    protected readonly TSettings settings = settings;
    protected readonly IQuery query = query;
    protected readonly List<QueryParameter> parameters = [];
    protected Dictionary<string, object?> outputDict = new(StringComparer.InvariantCultureIgnoreCase);

    public abstract DbConnection CreateConnection();

    public virtual DbCommand CreateCommand(DbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        var command = connection.CreateCommand();
        parameters.ForEach(param => command.Parameters.Add(CreateParameter(command, param)));
        command.CommandTimeout = (int)settings.CommandTimeout.TotalSeconds;
        command.CommandText = query.GetText(parameters);
        command.CommandType = query is IStoredProcedureQuery
            ? CommandType.StoredProcedure
            : CommandType.Text;
        return command;
    }

    public virtual DbParameter CreateParameter(DbCommand command, QueryParameter qParam)
    {
        var dbParam = command.CreateParameter();
        dbParam.ParameterName = $"@{qParam.Name.TrimStart('@')}";
        dbParam.Value = qParam.Value ?? DBNull.Value;
        dbParam.Direction = qParam.Direction;
        dbParam.DbType = qParam.DbType;
        dbParam.Size = qParam.Size ?? dbParam.Size;
        dbParam.Scale = qParam.Scale ?? dbParam.Scale;
        dbParam.Precision = qParam.Precision ?? dbParam.Precision;
        return dbParam;
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

    protected virtual DbConnection Connect()
    {
        var connection = CreateConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        return connection;
    }

    protected virtual async ValueTask<DbConnection> ConnectAsync(CancellationToken cancellation)
    {
        var connection = CreateConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellation);
        }

        return connection;
    }

    protected async ValueTask TryExecuteStoredProcedureWithOutputAsync(CancellationToken cancellationToken)
    {
        if (outputDict.Count == 0)
        {
            await GetAsync<int>(cancellationToken);
        }
    }

    protected void TryPrepareStoredProcedureOutput(DbCommand command)
    {
        foreach (var param in parameters
            .Where(p => p.Direction is ParameterDirection.Output or ParameterDirection.InputOutput or ParameterDirection.ReturnValue))
        {
            param.Value = command.Parameters[$"@{param.Name.TrimStart('@')}"].Value;
            outputDict[param.Name.TrimStart('@')] = param.Value;
        }
    }
}
