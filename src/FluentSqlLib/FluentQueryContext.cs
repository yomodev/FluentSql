
namespace FluentSqlLib;

public class FluentQueryContext(IFluentSqlClient client, string sql) : IFluentQueryContext
{
    // Add query operations here (Execute, WithParameter, etc.)
    public Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
