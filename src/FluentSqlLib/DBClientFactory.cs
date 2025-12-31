using Microsoft.Extensions.Logging;

namespace FluentSqlLib;

internal class DBClientFactory
{
    internal static ISqlClient Create<TSettings>(ILogger<TSettings> logger, TSettings settings, IQuery query)
        where TSettings : IFluentSqlSettings
    {
        return new SqlServerClient<TSettings>(logger, settings, query);
    }
}