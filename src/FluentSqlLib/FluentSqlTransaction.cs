namespace FluentSqlLib;

public class FluentSqlTransaction(IFluentSql client) : IFluentSqlTransaction
{
    private bool _transactionCompleted;
    private bool disposedValue;

    public void Commit()
    {
        if (!_transactionCompleted)
        {
            //transaction.Commit();
            _transactionCompleted = true;
        }
    }

    public void Rollback()
    {
        if (!_transactionCompleted)
        {
            //transaction.Rollback();
            _transactionCompleted = true;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue)
        {
            return;
        }

        if (disposing)
        {
            Rollback();
        }

        disposedValue = true;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
