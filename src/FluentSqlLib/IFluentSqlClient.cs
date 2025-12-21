namespace FluentSqlLib;

public interface IFluentSqlClient
{
    IFluentSqlTransaction BeginTransaction();

    IFluentDatabaseContext CurrentDatabase { get; }

    IFluentDatabaseContext Database(string name);

    IFluentDeleteQueryContext Delete(string name);

    void ExecuteTransaction(Action<IFluentSqlTransaction> action);

    IFluentInsertQueryContext Insert(string name);

    IAsyncEnumerable<string> ListDatabasesAsync(CancellationToken cancellationToken = default);
    
    IFluentQueryContext Query(string sql);
    
    IFluentSelectQueryContext Select(string name);
  
    IFluentStoredProcedureContext StoredProcedure(string name);
    
    IFluentTableContext Table(string name);
   
    IFluentUdfContext Udf(string name);

    IFluentUpdateQueryContext Update(string name);
}
