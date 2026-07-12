namespace FluentSqlLib;

/// <summary>
/// Synchronous counterparts to the async data-access members. These are true synchronous
/// implementations (real Open / ExecuteReader / Read calls), not GetAwaiter().GetResult()
/// wrappers over the async paths.
/// </summary>
public abstract partial class ClientBase<TSettings>
{
    public virtual IEnumerable<T> Enumerate<T>(bool skipMissingColumns = true) where T : new()
    {
        var behavior = CommandBehavior.SingleResult | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection;
        using var connection = Connect();
        using var command = CreateCommand(connection);
        using var reader = (SqlDataReader)command.ExecuteReader(behavior);
        TryPrepareStoredProcedureOutput(command);
        var mapper = RuntimeMapper.GetMapper<T>(reader, skipMissingColumns);
        while (reader.Read())
        {
            yield return mapper(reader);
        }
    }

    public virtual IEnumerable<T> Enumerate<T>(
        IReadOnlyDictionary<string, string> propertyToColumn, bool skipMissingColumns = true) where T : new()
    {
        var behavior = CommandBehavior.SingleResult | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection;
        using var connection = Connect();
        using var command = CreateCommand(connection);
        using var reader = (SqlDataReader)command.ExecuteReader(behavior);
        TryPrepareStoredProcedureOutput(command);
        var mapper = RuntimeMapper.GetMapper<T>(reader, propertyToColumn, skipMissingColumns);
        while (reader.Read())
        {
            yield return mapper(reader);
        }
    }

    public virtual IEnumerable<T> Enumerate<T>(
        IReadOnlyDictionary<string, Action<T, IDataRecord>> columnSetters, bool skipMissingColumns = true) where T : new()
    {
        // Buffered (not SequentialAccess): the caller's setters read columns in any order.
        var behavior = CommandBehavior.SingleResult | CommandBehavior.CloseConnection;
        using var connection = Connect();
        using var command = CreateCommand(connection);
        using var reader = (SqlDataReader)command.ExecuteReader(behavior);
        TryPrepareStoredProcedureOutput(command);

        var activeSetters = new List<Action<T, IDataRecord>>(columnSetters.Count);
        foreach (var (columnName, setter) in columnSetters)
        {
            try
            {
                reader.GetOrdinal(columnName);
            }
            catch (IndexOutOfRangeException)
            {
                if (!skipMissingColumns)
                {
                    throw new InvalidOperationException($"Column '{columnName}' was not found in the result set.");
                }

                continue;
            }

            activeSetters.Add(setter);
        }

        while (reader.Read())
        {
            var item = new T();
            foreach (var setter in activeSetters)
            {
                setter(item, reader);
            }

            yield return item;
        }
    }

    public virtual IEnumerable<T> Enumerate<T>(Func<IDataRecord, T> mapper)
    {
        var behavior = CommandBehavior.SingleResult | CommandBehavior.CloseConnection;
        using var connection = Connect();
        using var command = CreateCommand(connection);
        using var reader = (SqlDataReader)command.ExecuteReader(behavior);
        TryPrepareStoredProcedureOutput(command);
        while (reader.Read())
        {
            yield return mapper(reader);
        }
    }

    public virtual T Get<T>()
    {
        using var connection = Connect();
        using var command = CreateCommand(connection);

        if (query is IStoredProcedureQuery)
        {
            if (typeof(T) != typeof(int))
            {
                throw new InvalidOperationException("Return type for stored procedure must be Int32 when calling Get without column parameter.");
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
            return Mapper.MapScalar<T>(result);
        }

        throw new NotSupportedException("Get without column parameter is only supported for stored procedures and functions.");
    }

    public virtual IMultipleResultReader QueryMultiple()
    {
        var connection = Connect();
        var command = CreateCommand(connection);
        var reader = (SqlDataReader)command.ExecuteReader();
        return new MultipleResultReader(connection, command, reader);
    }

    public virtual T? Get<T>(string column)
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

        return default;
    }

    public virtual T Get<T>(string column, T defaultValue)
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

        return defaultValue;
    }

    public virtual IReadOnlyDictionary<string, object?> GetOutput()
    {
        if (outputDict.Count == 0)
        {
            GetRequired<int>();
        }

        return outputDict.ToDictionary();
    }

    public virtual T GetOutput<T>()
    {
        if (outputDict.Count == 0)
        {
            GetRequired<int>();
        }

        if (outputDict.Count > 0)
        {
            return Mapper.MapScalar<T>(outputDict.Values.First());
        }

        throw new InvalidOperationException("No output found.");
    }

    public virtual T? GetOutput<T>(string column)
    {
        if (outputDict.Count == 0)
        {
            GetRequired<int>();
        }

        if (outputDict.TryGetValue(column, out var value))
        {
            return Mapper.MapScalar<T>(value);
        }

        throw new InvalidOperationException("No output found.");
    }

    public virtual T GetOutput<T>(string column, T defaultValue)
    {
        if (outputDict.Count == 0)
        {
            GetRequired<int>();
        }

        return outputDict.TryGetValue(column, out var value)
            ? Mapper.MapScalar<T>(value)
            : defaultValue;
    }

    public virtual long InsertMany<T>(IEnumerable<T> rows, int? batchSize = null)
    {
        var tableName = query.GetText(parameters);
        using var connection = (SqlConnection)Connect();
        using var reader = new EnumerableDataReader<T>(rows);

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = tableName
        };

        if (batchSize.HasValue)
        {
            bulkCopy.BatchSize = batchSize.Value;
        }

        for (int i = 0; i < reader.Columns.Count; i++)
        {
            bulkCopy.ColumnMappings.Add(i, reader.Columns[i].ColumnName);
        }

        bulkCopy.WriteToServer(reader);
        connection.Close();
        return reader.RowsRead;
    }
}
