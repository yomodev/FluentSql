namespace FluentSqlLib;

/// <summary>Scalar / single-value retrieval: GetAsync and GetRequired families.</summary>
public abstract partial class ClientBase<TSettings>
{
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
            command.Parameters.Add(CreateParameter(command, param));
            await command.ExecuteNonQueryAsync(cancellationToken);
            TryPrepareStoredProcedureOutput(command);
            connection.Close();
            return (T)param.Value!;
        }
        else if (query is IFunctionQuery function)
        {
            command.CommandText = function.GetScalarFunctionText(parameters);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            connection.Close();
            return Mapper.MapScalar<T>(result);
        }

        throw new NotSupportedException("GetAsync without column parameter is only supported for stored procedures and functions.");
    }

    public virtual async ValueTask<T?> GetAsync<T>(
        string column, CancellationToken cancellationToken = default)
    {
        using var connection = await ConnectAsync(cancellationToken);
        using var command = CreateCommand(connection);
        using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            var ordinal = reader.GetOrdinal(column);
            if (!reader.IsDBNull(ordinal))
            {
                return Mapper.MapScalar<T>(reader.GetValue(ordinal));
            }
        }

        return default;
    }

    public virtual async ValueTask<T> GetAsync<T>(
        string column, T defaultValue, CancellationToken cancellationToken = default)
    {
        using var connection = await ConnectAsync(cancellationToken);
        using var command = CreateCommand(connection);
        using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            var ordinal = reader.GetOrdinal(column);
            if (!reader.IsDBNull(ordinal))
            {
                return Mapper.MapScalar<T>(reader.GetValue(ordinal));
            }
        }

        return defaultValue;
    }

    public virtual T GetRequired<T>()
    {
        using var connection = Connect();
        using var command = CreateCommand(connection);

        if (query is IStoredProcedureQuery)
        {
            if (typeof(T) != typeof(int))
            {
                throw new InvalidOperationException("Return type for stored procedure must be Int32 when calling GetRequired without column parameter.");
            }

            var param = new QueryParameter<int>("@ReturnValue", DbType.Int32)
            { Direction = ParameterDirection.ReturnValue };
            parameters.Add(param);
            command.Parameters.Add(CreateParameter(command, param));
            command.ExecuteNonQuery();
            TryPrepareStoredProcedureOutput(command);
            connection.Close();
            return (T)param.Value!;
        }
        else if (query is IFunctionQuery function)
        {
            command.CommandText = function.GetScalarFunctionText(parameters);
            var result = command.ExecuteScalar();
            connection.Close();
            if (result is null || result == DBNull.Value)
            {
                throw new InvalidOperationException("Required scalar value was null.");
            }

            return Mapper.MapScalar<T>(result);
        }

        throw new NotSupportedException("GetRequired without column parameter is only supported for stored procedures and functions.");
    }

    public virtual T GetRequired<T>(string column)
    {
        using var connection = Connect();
        using var command = CreateCommand(connection);
        using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
        if (reader.Read())
        {
            var ordinal = reader.GetOrdinal(column);
            if (!reader.IsDBNull(ordinal))
            {
                return Mapper.MapScalar<T>(reader.GetValue(ordinal));
            }
        }

        throw new InvalidOperationException($"Required column '{column}' was null or the query returned no rows.");
    }

    public virtual async ValueTask<T> GetRequiredAsync<T>(CancellationToken cancellationToken = default)
    {
        var value = await GetAsync<T>(cancellationToken);
        if (value is null)
        {
            throw new InvalidOperationException("Required scalar value was null.");
        }

        return value;
    }

    public virtual async ValueTask<T> GetRequiredAsync<T>(
        string column, CancellationToken cancellationToken = default)
    {
        var value = await GetAsync<T>(column, cancellationToken);
        if (value is null)
        {
            throw new InvalidOperationException($"Required column '{column}' was null or the query returned no rows.");
        }

        return value;
    }
}
