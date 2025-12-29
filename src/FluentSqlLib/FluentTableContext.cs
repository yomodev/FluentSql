
namespace FluentSqlLib;

public class FluentTableContext(IFluentSql fluentSql, string tableName) : IFluentTableContext
{
    public ValueTask<long> InsertManyAsync<T>(IEnumerable<T> rows, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
