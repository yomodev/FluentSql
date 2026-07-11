namespace FluentSqlLib;

/// <summary>
/// Streaming bulk insert via <see cref="SqlBulkCopy"/> over <see cref="EnumerableDataReader{T}"/> -
/// no DataTable, forward-only.
/// </summary>
public abstract partial class ClientBase<TSettings>
{
    public virtual async ValueTask<long> InsertManyAsync<T>(
        IEnumerable<T> rows, int? batchSize = null, CancellationToken cancellationToken = default)
    {
        var tableName = query.GetText(parameters);
        using var connection = (SqlConnection)await ConnectAsync(cancellationToken);
        using var reader = new EnumerableDataReader<T>(rows);

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = tableName
        };

        if (batchSize.HasValue)
        {
            bulkCopy.BatchSize = batchSize.Value;
        }

        for (int i = 0; i < reader.Columns.Count; i++)
        {
            bulkCopy.ColumnMappings.Add(i, reader.Columns[i].ColumnName);
        }

        await bulkCopy.WriteToServerAsync(reader, cancellationToken);
        connection.Close();
        return reader.RowsRead;
    }
}
