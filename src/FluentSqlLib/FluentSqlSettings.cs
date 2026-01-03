namespace FluentSqlLib;

public class FluentSqlSettings : IFluentSqlSettings
{
    public required string ConnectionString { get; set; }

    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
}