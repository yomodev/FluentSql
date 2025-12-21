namespace FluentSqlLib;

public interface IFluentSqlTransaction : IDisposable
{
    void Commit();
    void Rollback();
}