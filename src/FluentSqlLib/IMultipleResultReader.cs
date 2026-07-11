namespace FluentSqlLib;

/// <summary>
/// Forward-only reader over a command that returns several result sets. Consume the grids in the
/// order the command emits them: each <see cref="ReadAsync{T}"/> / <see cref="ReadScalarAsync{T}"/>
/// call drains the current grid and advances to the next. A grid must be fully enumerated before
/// requesting the next one, and reading past the last grid throws.
/// </summary>
public interface IMultipleResultReader : IAsyncDisposable
{
    /// <summary>Streams the current result set as <typeparamref name="T"/>, then advances to the next grid.</summary>
    IAsyncEnumerable<T> ReadAsync<T>(
        bool skipMissingColumns = true,
        CancellationToken cancellationToken = default) where T : new();

    /// <summary>Materializes the current result set as a list, then advances to the next grid.</summary>
    ValueTask<IReadOnlyList<T>> ReadListAsync<T>(
        bool skipMissingColumns = true,
        CancellationToken cancellationToken = default) where T : new();

    /// <summary>Reads the first column of the first row of the current result set, then advances to the next grid.</summary>
    ValueTask<T?> ReadScalarAsync<T>(CancellationToken cancellationToken = default);
}
