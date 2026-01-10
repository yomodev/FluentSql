namespace FluentSqlLib;

public interface IMultipleRowsResult
{
    IAsyncEnumerable<T> EnumerateAsync<T>(CancellationToken cancellationToken);
}