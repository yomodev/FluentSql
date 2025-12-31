namespace FluentSqlLib;

internal class NoResultQuery(string sql) : IQuery
{
    public override string ToString() => sql;
}