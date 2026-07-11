
namespace FluentSqlLib;

public class FluentTableContext(IFluentSql fluentSql, string tableName) : IFluentTableContext
{
    public async ValueTask<long> InsertManyAsync<T>(
        IEnumerable<T> rows, int? batchSize = null, CancellationToken cancellationToken = default)
    {
        using var client = fluentSql.CreateClient(new NoResultQuery(tableName));
        return await client.InsertManyAsync(rows, batchSize, cancellationToken);
    }
}
