using Microsoft.Extensions.Logging;

namespace FluentSqlLib;

internal class FluentSpClient<TSettings>(
    ILogger<TSettings> logger,
    TSettings settings,
    string procedureName) 
    : FluentSqlClient<TSettings>(logger, settings, $"EXEC {procedureName}")
    where TSettings : IFluentSqlSettings
{
}