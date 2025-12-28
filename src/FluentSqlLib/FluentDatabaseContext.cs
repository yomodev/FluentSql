namespace FluentSqlLib;

public class FluentDatabaseContext(IFluentSql fluentSql, string databaseName) : IFluentDatabaseContext
{
    public string Name => databaseName;

    // Add database operations here (Create, Drop, Exists, ListTables, etc.)
}
