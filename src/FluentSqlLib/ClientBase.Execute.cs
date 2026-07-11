namespace FluentSqlLib;

/// <summary>Non-query execution returning rows-affected.</summary>
public abstract partial class ClientBase<TSettings>
{
    // return the number of rows affected
    public virtual int Execute()
    {
        using var connection = Connect();
        using var command = CreateCommand(connection);
        var result = command.ExecuteNonQuery();
            TryPrepareStoredProcedureOutput(command);
        connection.Close();
        return result;
    }

    // return the number of rows affected
    public virtual async ValueTask<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await ConnectAsync(cancellationToken);
        using var command = CreateCommand(connection);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
            TryPrepareStoredProcedureOutput(command);
        connection.Close();
        return result;
    }
}
