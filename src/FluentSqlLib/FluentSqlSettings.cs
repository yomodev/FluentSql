namespace FluentSqlLib;

public class FluentSqlSettings : IFluentSqlSettings
{
    public required string ConnectionString { get; set; }
}