
namespace FluentSqlLib;

public class FluentQueryContext(IFluentSql fluentSql, string sql) 
    : IFluentQueryContext, IFluentDeleteQueryContext, IFluentInsertQueryContext, 
    IFluentSelectQueryContext, IFluentUpdateQueryContext
{
    // Add query operations here (Execute, WithParameter, etc.)
    public ValueTask<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
