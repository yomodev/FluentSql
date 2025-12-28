
namespace FluentSqlLib;

public class FluentTableContext(IFluentSql fluentSql, string tableName) : IFluentTableContext
{
    public ValueTask<bool> DropAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<long> InsertManyAsync<T>(IEnumerable<T> rows, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
