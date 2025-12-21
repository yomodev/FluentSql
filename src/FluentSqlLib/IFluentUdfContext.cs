using FluentSqlLib.Interfaces;

namespace FluentSqlLib;

public interface IFluentUdfContext : IUdfInputParam
{
}

public interface IUdfInputParam : IUdfNoParam
{
    IUdfInputParam WithParam<T>(string paramName, T value);
}

public interface IUdfNoParam : ISingleRowResult, IMultipleRowsResult
{
}