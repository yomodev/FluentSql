TODO

FluentSql should return the right IFluentSqlClient implementation for the given 
database provider.

there is a ClientBase class that implements common functionality, and then
a MySqlClient, PostgresClient, SqlServerClient, SqliteClient that can inherit from it.