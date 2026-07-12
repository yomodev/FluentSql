
namespace FluentSqlLib;

public class FluentTableContext(IFluentSql fluentSql, string tableName) : IFluentTableContext
{
    public async ValueTask<long> InsertManyAsync<T>(
        IEnumerable<T> rows, int? batchSize = null, CancellationToken cancellationToken = default)
    {
        using var client = fluentSql.CreateClient(new NoResultQuery(tableName));
        return await client.InsertManyAsync(rows, batchSize, cancellationToken);
    }

    public long InsertMany<T>(IEnumerable<T> rows, int? batchSize = null)
    {
        using var client = fluentSql.CreateClient(new NoResultQuery(tableName));
        return client.InsertMany(rows, batchSize);
    }
}
