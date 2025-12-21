namespace FluentSqlLib.Interfaces;

public interface IMultipleRowsResult
{
    IAsyncEnumerable<T> EnumerateAsync<T>(CancellationToken cancellationToken);
}