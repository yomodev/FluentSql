namespace FluentSqlLib;

/// <summary>
/// A function call context. Add parameters with <c>WithParam</c>, then read a scalar function's
/// value with <c>GetAsync&lt;T&gt;</c>/<c>Get&lt;T&gt;</c> or a table-valued function's rows with
/// <c>EnumerateAsync&lt;T&gt;</c>/<c>Enumerate&lt;T&gt;</c>.
/// </summary>
public interface IFluentFunctionContext : IFunctionInputParam
{
}

public interface IFunctionInputParam : IFunctionNoParam
{
    IFunctionInputParam WithParam<T>(string name, T value);
}

public interface IFunctionNoParam : ISingleRowResult, IMultipleRowsResult
{
}