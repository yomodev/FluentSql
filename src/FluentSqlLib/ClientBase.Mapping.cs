using System.Runtime.CompilerServices;

namespace FluentSqlLib;

/// <summary>
/// Row enumeration: raw <see cref="IDataReader"/> streaming plus the typed EnumerateAsync&lt;T&gt;
/// overloads (convention/attribute, property-to-column map, column-to-setter map, and custom mapper).
/// </summary>
public abstract partial class ClientBase<TSettings>
{
    public virtual IEnumerable<IDataReader> Enumerate()
    {
        var behavior = CommandBehavior.CloseConnection;
        using var connection = Connect();
        using var command = CreateCommand(connection);
        using var reader = command!.ExecuteReader(behavior);
            TryPrepareStoredProcedureOutput(command);
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
            TryPrepareStoredProcedureOutput(command);
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

    public virtual async IAsyncEnumerable<T> EnumerateAsync<T>(
        bool skipMissingColumns = true,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : new()
    {
        var behavior = CommandBehavior.SingleResult | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection;
        using var connection = await ConnectAsync(cancellationToken);
        using var command = CreateCommand(connection);
        using var reader = (SqlDataReader)await command.ExecuteReaderAsync(behavior, cancellationToken);
        TryPrepareStoredProcedureOutput(command);
        var mapper = RuntimeMapper.GetMapper<T>(reader, skipMissingColumns);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return mapper(reader);
        }
    }

    public virtual async IAsyncEnumerable<T> EnumerateAsync<T>(
        IReadOnlyDictionary<string, string> propertyToColumn,
        bool skipMissingColumns = true,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : new()
    {
        var behavior = CommandBehavior.SingleResult | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection;
        using var connection = await ConnectAsync(cancellationToken);
        using var command = CreateCommand(connection);
        using var reader = (SqlDataReader)await command.ExecuteReaderAsync(behavior, cancellationToken);
        TryPrepareStoredProcedureOutput(command);
        var mapper = RuntimeMapper.GetMapper<T>(reader, propertyToColumn, skipMissingColumns);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return mapper(reader);
        }
    }

    public virtual async IAsyncEnumerable<T> EnumerateAsync<T>(
        IReadOnlyDictionary<string, Action<T, IDataRecord>> columnSetters,
        bool skipMissingColumns = true,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : new()
    {
        var behavior = CommandBehavior.SingleResult | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection;
        using var connection = await ConnectAsync(cancellationToken);
        using var command = CreateCommand(connection);
        using var reader = (SqlDataReader)await command.ExecuteReaderAsync(behavior, cancellationToken);
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

        while (await reader.ReadAsync(cancellationToken))
        {
            var item = new T();
            foreach (var setter in activeSetters)
            {
                setter(item, reader);
            }

            yield return item;
        }
    }

    public virtual async IAsyncEnumerable<T> EnumerateAsync<T>(
        Func<IDataRecord, T> mapper,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var behavior = CommandBehavior.SingleResult | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection;
        using var connection = await ConnectAsync(cancellationToken);
        using var command = CreateCommand(connection);
        using var reader = (SqlDataReader)await command.ExecuteReaderAsync(behavior, cancellationToken);
        TryPrepareStoredProcedureOutput(command);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return mapper(reader);
        }
    }
}
