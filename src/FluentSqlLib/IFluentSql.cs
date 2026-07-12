namespace FluentSqlLib;

/// <summary>
/// The root entry point of the fluent API. Create one per configured connection and use its
/// factory methods to start a query, call a stored procedure or function, bulk-insert into a
/// table, or inspect database metadata.
/// </summary>
public interface IFluentSql
{
    /// <summary>The database named in the connection string, for metadata and DDL operations.</summary>
    IFluentDatabaseContext CurrentDatabase { get; }

    /// <summary>Begins a transaction. (Commit/Rollback wiring is not yet implemented.)</summary>
    IFluentSqlTransaction BeginTransaction();

    /// <summary>Creates a low-level client for an arbitrary <see cref="IQuery"/>.</summary>
    ISqlClient CreateClient(IQuery query);

    /// <summary>Creates a low-level client for a raw SQL string.</summary>
    ISqlClient CreateClient(string query);

    /// <summary>Targets a specific database by name for metadata and DDL operations.</summary>
    IFluentDatabaseContext Database(string name);

    /// <summary>Starts a DELETE statement context.</summary>
    IFluentDeleteQueryContext Delete(string name);

    /// <summary>Runs the action inside a transaction, committing on success and rolling back on exception.</summary>
    void ExecuteTransaction(Action<IFluentSqlTransaction> action);

    /// <summary>Starts an INSERT statement context.</summary>
    IFluentInsertQueryContext Insert(string name);

    /// <summary>Streams the names of all databases on the server.</summary>
    IAsyncEnumerable<string> ListDatabasesAsync(CancellationToken cancellationToken = default);

    /// <summary>Starts a raw-SQL query context (SELECT or otherwise) from a full SQL string.</summary>
    IFluentQueryContext Query(string sql);

    /// <summary>Starts a SELECT query context. Pass the SQL to run; map results with QueryAsync/Query&lt;T&gt;.</summary>
    IFluentSelectQueryContext Select(string name);

    /// <summary>Starts a stored-procedure call context.</summary>
    IFluentStoredProcedureContext StoredProcedure(string name);

    /// <summary>Starts a table context, e.g. for bulk insert via InsertManyAsync/InsertMany.</summary>
    IFluentTableContext Table(string name);

    /// <summary>Starts a scalar or table-valued function call context.</summary>
    IFluentFunctionContext Function(string name);

    /// <summary>Starts an UPDATE statement context.</summary>
    IFluentUpdateQueryContext Update(string name);
}
