namespace FluentSqlLib;

public class FluentDatabaseContext(IFluentSql fluentSql, string databaseName) : IFluentDatabaseContext
{
    // Add database operations here (Create, Drop, Exists, ListTables, etc.)
}
