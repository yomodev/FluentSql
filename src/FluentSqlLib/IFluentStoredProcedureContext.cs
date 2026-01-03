using FluentSqlLib.Interfaces;

namespace FluentSqlLib;

public interface IFluentStoredProcedureContext: ISpParam
{
}

public interface ISpParam : ISpNoParam
{
    ISpParam WithParam<T>(string name, T value);
    
    ISpParam WithParam<T>(string name, IEnumerable<T> tableValued, string tableTypeName);

    ISpParam WithOutputParam<T>(string name, T value);
}

public interface ISpNoParam : ISingleRowResult, IMultipleRowsResult, IMultipleResults
{
    ValueTask<IReadOnlyDictionary<string, object?>> GetOutputAsync(CancellationToken cancellationToken = default);

    ValueTask<T> GetOutputAsync<T>(CancellationToken cancellationToken = default);

    ValueTask<T?> GetOutputAsync<T>(string column, CancellationToken cancellationToken = default);

    ValueTask<T> GetOutputAsync<T>(string column, T defaultValue, CancellationToken cancellationToken = default);
}