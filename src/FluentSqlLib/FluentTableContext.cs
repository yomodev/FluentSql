
namespace FluentSqlLib;

public class FluentTableContext(IFluentSqlClient client, string tableName) : IFluentTableContext
{
    public Task<bool> DropAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<long> InsertManyAsync<T>(IEnumerable<T> rows, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
