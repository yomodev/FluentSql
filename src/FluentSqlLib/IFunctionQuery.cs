namespace FluentSqlLib;

public interface IFunctionQuery : IQuery
{
    string GetScalarFunctionText(IReadOnlyList<QueryParameter> parameters);
}