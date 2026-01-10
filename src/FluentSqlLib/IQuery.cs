namespace FluentSqlLib;

public interface IQuery
{
    string GetText(IReadOnlyList<QueryParameter> parameters);
}