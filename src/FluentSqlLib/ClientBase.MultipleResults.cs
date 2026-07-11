namespace FluentSqlLib;

/// <summary>Multi-result-set access (see <see cref="IMultipleResultReader"/>).</summary>
public abstract partial class ClientBase<TSettings>
{
    public virtual async ValueTask<IMultipleResultReader> QueryMultipleAsync(
        CancellationToken cancellationToken = default)
    {
        // The returned reader takes ownership of the connection/command/reader and disposes them.
        var connection = await ConnectAsync(cancellationToken);
        var command = CreateCommand(connection);
        var reader = (SqlDataReader)await command.ExecuteReaderAsync(cancellationToken);
        return new MultipleResultReader(connection, command, reader);
    }
}
