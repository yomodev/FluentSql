namespace FluentSqlLib;

/// <summary>
/// A table context for high-throughput operations. <see cref="InsertManyAsync"/> streams rows into
/// the table via SqlBulkCopy without materializing a DataTable; <see cref="SqlColumnAttribute.Identity"/>
/// and <see cref="SqlColumnAttribute.Computed"/> properties are excluded from the insert.
/// </summary>
public interface IFluentTableContext
{
    /// <summary>Bulk-inserts <paramref name="rows"/>, optionally batched, and returns the count written.</summary>
    ValueTask<long> InsertManyAsync<T>(IEnumerable<T> rows, int? batchSize = null, CancellationToken cancellationToken = default);

    /// <summary>Synchronous counterpart to <see cref="InsertManyAsync"/>.</summary>
    long InsertMany<T>(IEnumerable<T> rows, int? batchSize = null);
}