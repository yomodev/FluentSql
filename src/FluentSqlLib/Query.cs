namespace FluentSqlLib;

internal class Query(string query) : IQuery
{
    public virtual string GetText(IReadOnlyList<QueryParameter> parameters) => query;
}