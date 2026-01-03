using FluentSqlLib.Interfaces;

namespace FluentSqlLib;

public interface IFluentFunctionContext : IFunctionInputParam
{
}

public interface IFunctionInputParam : IFunctinNoParam
{
    IFunctionInputParam WithParam<T>(string name, T value);
}

public interface IFunctinNoParam : ISingleRowResult, IMultipleRowsResult
{
}