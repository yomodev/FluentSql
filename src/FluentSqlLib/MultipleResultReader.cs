using System.Data.Common;
using System.Runtime.CompilerServices;

namespace FluentSqlLib;

/// <summary>
/// Default <see cref="IMultipleResultReader"/>: owns the connection/command/reader opened for a
/// multi-result command and disposes them when the reader is disposed.
/// </summary>
internal sealed class MultipleResultReader(
    DbConnection connection, DbCommand command, SqlDataReader reader) : IMultipleResultReader
{
    private bool _hasMoreResults = true;
    private bool _disposed;

    public async IAsyncEnumerable<T> ReadAsync<T>(
        bool skipMissingColumns = true,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : new()
    {
        EnsureMoreResults();
        var mapper = RuntimeMapper.GetMapper<T>(reader, skipMissingColumns);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return mapper(reader);
        }

        _hasMoreResults = await reader.NextResultAsync(cancellationToken);
    }

    public async ValueTask<IReadOnlyList<T>> ReadListAsync<T>(
        bool skipMissingColumns = true, CancellationToken cancellationToken = default) where T : new()
    {
        var list = new List<T>();
        await foreach (var item in ReadAsync<T>(skipMissingColumns, cancellationToken))
        {
            list.Add(item);
        }

        return list;
    }

    public async ValueTask<T?> ReadScalarAsync<T>(CancellationToken cancellationToken = default)
    {
        EnsureMoreResults();
        T? result = default;
        if (await reader.ReadAsync(cancellationToken) && !reader.IsDBNull(0))
        {
            result = Mapper.MapScalar<T>(reader.GetValue(0));
        }

        // Drain any remaining rows of this grid before advancing.
        while (await reader.ReadAsync(cancellationToken))
        {
        }

        _hasMoreResults = await reader.NextResultAsync(cancellationToken);
        return result;
    }

    private void EnsureMoreResults()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_hasMoreResults)
        {
            throw new InvalidOperationException(
                "No more result sets are available. Read result sets in order, once each, without reading past the last.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await reader.DisposeAsync();
        await command.DisposeAsync();
        await connection.DisposeAsync();
    }
}
