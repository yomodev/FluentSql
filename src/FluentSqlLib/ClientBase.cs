using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace FluentSqlLib;

public abstract class ClientBase<TSettings>(
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

    public virtual IEnumerable<IDataReader> Enumerate()
    {
        var behavior = CommandBehavior.CloseConnection;
        using var connection = Connect();
        using var command = CreateCommand(connection);
        using var reader = command!.ExecuteReader(behavior);
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
        using var connection = await ConnectAsync(cancellationToken);
        using var command = CreateCommand(connection);
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
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

    // return the number of rows affected
    public virtual int Execute()
    {
        using var connection = Connect();
        using var command = CreateCommand(connection);
        var result = command.ExecuteNonQuery();
        connection.Close();
        return result;
    }

    // return the number of rows affected
    public virtual async ValueTask<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await ConnectAsync(cancellationToken);
        using var command = CreateCommand(connection);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        connection.Close();
        return result;
    }

    public virtual async ValueTask<T> GetAsync<T>(CancellationToken cancellationToken = default)
    {
        using var connection = await ConnectAsync(cancellationToken);
        using var command = CreateCommand(connection);

        if (query is IStoredProcedureQuery)
        {
            if (typeof(T) != typeof(Int32))
            {
                throw new InvalidOperationException("Return type for stored procedure must be Int32 when calling GetAsync without column parameter.");
            }

            var param = new QueryParameter<int>("@ReturnValue", DbType.Int32)
            { Direction = ParameterDirection.ReturnValue };
            parameters.Add(param);
            await command.ExecuteNonQueryAsync(cancellationToken);
            connection.Close();
            return (T)param.Value!;
        }
        else if (query is IFunctionQuery function)
        {
            command.CommandText = function.GetScalarFunctionText(parameters);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            connection.Close();
            return MapScalar<T>(result);
        }

        throw new NotSupportedException("GetAsync without column parameter is only supported for stored procedures and functions.");
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

    protected T MapScalar<T>(object? scalar)
    {
        T result = default!;
        if (scalar is null || scalar == DBNull.Value)
        {
            if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) is null)
            {
                throw new InvalidOperationException($"Cannot convert null to non-nullable type {typeof(T).FullName}.");
            }

            return result;
        }

        return (T)Convert.ChangeType(scalar, typeof(T));
    }
}
