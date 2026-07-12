namespace FluentSqlLib;

public interface IMultipleResults
{
    /// <summary>
    /// Opens a forward-only reader over a command that returns several result sets.
    /// Dispose the returned reader when done; it owns the underlying connection.
    /// </summary>
    ValueTask<IMultipleResultReader> QueryMultipleAsync(CancellationToken cancellationToken = default);

    /// <summary>Synchronous counterpart to <see cref="QueryMultipleAsync"/>.</summary>
    IMultipleResultReader QueryMultiple();
}