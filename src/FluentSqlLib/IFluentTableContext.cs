namespace FluentSqlLib;

public interface IFluentTableContext
{
    ValueTask<long> InsertManyAsync<T>(IEnumerable<T> rows, int? batchSize = null, CancellationToken cancellationToken = default);

    long InsertMany<T>(IEnumerable<T> rows, int? batchSize = null);
}