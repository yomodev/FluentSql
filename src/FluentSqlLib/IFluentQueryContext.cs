namespace FluentSqlLib;

public interface IFluentQueryContext
{
    /// <summary>
    /// Executes the operation asynchronously and returns the result as an integer.
    /// Return the number of affected rows or an exception (-1 if the operation does not affect rows).
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>An integer representing the result of the operation.</returns>
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
}