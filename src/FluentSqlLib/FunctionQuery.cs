namespace FluentSqlLib;

internal class FunctionQuery(string functionName)
    : Query(functionName), IFunctionQuery
{
    private readonly string functionName = functionName;

    public string GetScalarFunctionText(IReadOnlyList<QueryParameter> parameters)
    {
        return $"SELECT {functionName}({ParameterList(parameters)})";
    }

    override public string GetText(IReadOnlyList<QueryParameter> parameters)
    {
        return $"SELECT * FROM {functionName}({ParameterList(parameters)})";
    }

    private static string ParameterList(IReadOnlyList<QueryParameter> parameters)
    {
        return string.Join(", ", parameters.Select(p => "@" + p.Name));
    }
}