namespace FluentSqlLib;

internal class Query(string query) : IQuery
{
    public override string ToString()=> query;
}