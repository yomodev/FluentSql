namespace FluentSqlLib;

public interface IFluentSqlClient : IDisposable
{
    IAsyncEnumerable<IDataReader> EnumerateAsync(
        CommandBehavior behavior,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IDataReader> EnumerateAsync(
        CancellationToken cancellationToken = default);
}
