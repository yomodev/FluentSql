namespace FluentSqlLib;

/// <summary>Stored-procedure output-parameter retrieval.</summary>
public abstract partial class ClientBase<TSettings>
{
    public virtual async ValueTask<IReadOnlyDictionary<string, object?>> GetOutputAsync(
        CancellationToken cancellationToken = default)
    {
        await TryExecuteStoredProcedureWithOutputAsync(cancellationToken);

        return outputDict.ToDictionary();
    }

    public virtual async ValueTask<T> GetOutputAsync<T>(CancellationToken cancellationToken = default)
    {
        await TryExecuteStoredProcedureWithOutputAsync(cancellationToken);

        if (outputDict.Count > 0)
        {
            var value = outputDict.Values.First();
            return Mapper.MapScalar<T>(value);
        }

        throw new InvalidOperationException("No output found.");
    }

    public virtual async ValueTask<T?> GetOutputAsync<T>(
        string column, CancellationToken cancellationToken = default)
    {
        await TryExecuteStoredProcedureWithOutputAsync(cancellationToken);

        if (outputDict.TryGetValue(column, out var value))
        {
            return Mapper.MapScalar<T>(value);
        }

        throw new InvalidOperationException("No output found.");
    }

    public virtual async ValueTask<T> GetOutputAsync<T>(
        string column, T defaultValue, CancellationToken cancellationToken = default)
    {
        await TryExecuteStoredProcedureWithOutputAsync(cancellationToken);

        if (outputDict.TryGetValue(column, out var value))
        {
            return Mapper.MapScalar<T>(value);
        }

        return defaultValue;
    }
}
