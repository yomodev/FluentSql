namespace FluentSqlLib;

internal class StoredProcedureQuery(string procedureName) : IQuery
{
    public override string ToString() => $"EXEC {procedureName}";
}