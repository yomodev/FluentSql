using System.Runtime.CompilerServices;

namespace FluentSqlLib;

public class FluentQueryContext(IFluentSql fluentSql, string sql)
    : IFluentQueryContext, IFluentDeleteQueryContext, IFluentInsertQueryContext,
    IFluentSelectQueryContext, IFluentUpdateQueryContext
{
    private readonly List<Action<ISqlClient>> paramSetters = [];

    public async ValueTask<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await client.ExecuteAsync(cancellationToken);
    }

    public IFluentSelectQueryContext WithParam<T>(string name, T value)
    {
        paramSetters.Add(c => c.WithParam(name, value));
        return this;
    }

    public async IAsyncEnumerable<T> QueryAsync<T>(
        bool skipMissingColumns = true,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : new()
    {
        using var client = CreateClient();
        await foreach (var item in client.EnumerateAsync<T>(skipMissingColumns, cancellationToken))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<T> QueryAsync<T>(
        IReadOnlyDictionary<string, string> propertyToColumn,
        bool skipMissingColumns = true,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : new()
    {
        using var client = CreateClient();
        await foreach (var item in client.EnumerateAsync<T>(propertyToColumn, skipMissingColumns, cancellationToken))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<T> QueryAsync<T>(
        IReadOnlyDictionary<string, Action<T, IDataRecord>> columnSetters,
        bool skipMissingColumns = true,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : new()
    {
        using var client = CreateClient();
        await foreach (var item in client.EnumerateAsync<T>(columnSetters, skipMissingColumns, cancellationToken))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<T> QueryAsync<T>(
        Func<IDataRecord, T> mapper,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        await foreach (var item in client.EnumerateAsync<T>(mapper, cancellationToken))
        {
            yield return item;
        }
    }

    public async ValueTask<T?> GetAsync<T>(string column, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await client.GetAsync<T>(column, cancellationToken);
    }

    public async ValueTask<T> GetAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await client.GetAsync<T>(column, defaultValue, cancellationToken);
    }

    private ISqlClient CreateClient()
    {
        var client = fluentSql.CreateClient(sql);
        foreach (var setter in paramSetters)
        {
            setter(client);
        }

        return client;
    }
}
