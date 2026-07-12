namespace FluentSqlLib.Benchmarks;

/// <summary>
/// Creates and tears down a throwaway LocalDB database for the benchmarks, and holds the
/// wide, many-typed table used to exercise the full mapping/converter/bulk-insert path.
/// </summary>
internal sealed class BenchmarkDb : IDisposable
{
    private const string Master = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;";

    public string DatabaseName { get; } = $"FluentSqlBench_{Guid.NewGuid():N}";
    public string ConnectionString { get; }

    public BenchmarkDb()
    {
        ConnectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={DatabaseName};Integrated Security=true;TrustServerCertificate=True;";

        Exec(Master, $"CREATE DATABASE [{DatabaseName}];");
        Exec(ConnectionString, WideTableDdl);
    }

    public FluentSql<FluentSqlSettings> CreateFluent()
        => new(Microsoft.Extensions.Logging.Abstractions.NullLogger<FluentSqlSettings>.Instance,
            new FluentSqlSettings { ConnectionString = ConnectionString });

    public void Truncate() => Exec(ConnectionString, "TRUNCATE TABLE dbo.WideRows;");

    public void Dispose()
    {
        try
        {
            Exec(Master, $@"
                ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{DatabaseName}];");
        }
        catch
        {
            /* best-effort cleanup */
        }
    }

    private static void Exec(string connectionString, string sql)
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        foreach (var batch in sql.Split("GO\n", StringSplitOptions.RemoveEmptyEntries))
        {
            using var cmd = new SqlCommand(batch, conn);
            cmd.ExecuteNonQuery();
        }
    }

    private const string WideTableDdl = @"
        CREATE TABLE dbo.WideRows (
            Id            INT IDENTITY PRIMARY KEY,
            IntCol        INT NOT NULL,
            BigIntCol     BIGINT NOT NULL,
            BitCol        BIT NOT NULL,
            DecimalCol    DECIMAL(18,4) NOT NULL,
            FloatCol      FLOAT NOT NULL,
            RealCol       REAL NOT NULL,
            GuidCol       UNIQUEIDENTIFIER NOT NULL,
            DateCol       DATETIME2 NOT NULL,
            OffsetCol     DATETIMEOFFSET NOT NULL,
            TextCol       NVARCHAR(200) NULL,
            StatusCol     INT NOT NULL,     -- enum stored as int
            QuantityCol   INT NOT NULL,     -- custom converter type
            NullableIntCol INT NULL
        );";
}
