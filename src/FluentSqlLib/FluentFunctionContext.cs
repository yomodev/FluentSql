
namespace FluentSqlLib;

public class FluentFunctionContext(
    IFluentSql fluentSql, string functionName)
    : IFluentFunctionContext
{
    private readonly ISqlClient client = fluentSql.CreateClient(new FunctionQuery(functionName));

    public IAsyncEnumerable<T> EnumerateAsync<T>(
        bool skipMissingColumns = true, CancellationToken cancellationToken = default) where T : new()
        => client.EnumerateAsync<T>(skipMissingColumns, cancellationToken);

    public IAsyncEnumerable<T> EnumerateAsync<T>(
        IReadOnlyDictionary<string, string> propertyToColumn,
        bool skipMissingColumns = true, CancellationToken cancellationToken = default) where T : new()
        => client.EnumerateAsync<T>(propertyToColumn, skipMissingColumns, cancellationToken);

    public IAsyncEnumerable<T> EnumerateAsync<T>(
        IReadOnlyDictionary<string, Action<T, IDataRecord>> columnSetters,
        bool skipMissingColumns = true, CancellationToken cancellationToken = default) where T : new()
        => client.EnumerateAsync<T>(columnSetters, skipMissingColumns, cancellationToken);

    public IAsyncEnumerable<T> EnumerateAsync<T>(
        Func<IDataRecord, T> mapper, CancellationToken cancellationToken = default)
        => client.EnumerateAsync<T>(mapper, cancellationToken);

    public ValueTask<T> GetAsync<T>(
        CancellationToken cancellationToken = default)
        => client.GetAsync<T>(cancellationToken);

    public ValueTask<T?> GetAsync<T>(
        string column, CancellationToken cancellationToken = default)
        => client.GetAsync<T>(column, cancellationToken);

    public ValueTask<T> GetAsync<T>(
        string column, T defaultValue, CancellationToken cancellationToken = default)
        => client.GetAsync<T>(column, defaultValue, cancellationToken);

    public T GetRequired<T>()
        => client.GetRequired<T>();

    public T GetRequired<T>(string column)
        => client.GetRequired<T>(column);

    public ValueTask<T> GetRequiredAsync<T>(
        CancellationToken cancellationToken = default)
        => client.GetRequiredAsync<T>(cancellationToken);

    public ValueTask<T> GetRequiredAsync<T>(
        string column, CancellationToken cancellationToken = default)
        => client.GetRequiredAsync<T>(column, cancellationToken);

    public IFunctionInputParam WithParam<T>(
        string name, T value)
    {
        client.WithParam(name, value);
        return this;
    }
}