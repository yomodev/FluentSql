namespace FluentSqlLib;

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