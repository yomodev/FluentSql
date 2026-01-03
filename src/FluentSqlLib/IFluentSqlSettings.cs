namespace FluentSqlLib;

public interface IFluentSqlSettings
{
    public string ConnectionString { get; }

    public TimeSpan CommandTimeout { get; }
}