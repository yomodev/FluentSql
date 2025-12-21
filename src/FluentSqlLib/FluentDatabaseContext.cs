namespace FluentSqlLib;

public class FluentDatabaseContext(IFluentSqlClient client, string databaseName) : IFluentDatabaseContext
{
    // Add database operations here (Create, Drop, Exists, ListTables, etc.)
}
