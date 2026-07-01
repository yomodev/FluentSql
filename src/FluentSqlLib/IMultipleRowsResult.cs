namespace FluentSqlLib;

public interface IMultipleRowsResult
{
    IAsyncEnumerable<T> EnumerateAsync<T>(CancellationToken cancellationToken) where T : new();
}