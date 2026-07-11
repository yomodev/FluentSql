namespace FluentSqlLib;

/// <summary>
/// Provider-overridable DDL text generation. SQL Server syntax lives here as the default;
/// other providers override the individual builders (or <see cref="QuoteIdentifier"/>) as needed.
/// </summary>
public abstract partial class ClientBase<TSettings>
{
    public virtual string QuoteIdentifier(string identifier)
    {
        var parts = identifier.Split('.');
        return string.Join('.', parts.Select(p => $"[{p.Replace("]", "]]")}]"));
    }

    public virtual string BuildDropTableSql(string tableName)
        => $"DROP TABLE {QuoteIdentifier(tableName)}";

    public virtual string BuildDropIndexSql(string indexName, string tableName)
        => $"DROP INDEX {QuoteIdentifier(indexName)} ON {QuoteIdentifier(tableName)}";

    public virtual string BuildDropStoredProcedureSql(string procedureName)
        => $"DROP PROCEDURE {QuoteIdentifier(procedureName)}";

    public virtual string BuildDropFunctionSql(string functionName)
        => $"DROP FUNCTION {QuoteIdentifier(functionName)}";

    public virtual string BuildDropViewSql(string viewName)
        => $"DROP VIEW {QuoteIdentifier(viewName)}";

    public virtual string BuildTruncateTableSql(string tableName)
        => $"TRUNCATE TABLE {QuoteIdentifier(tableName)}";
}
