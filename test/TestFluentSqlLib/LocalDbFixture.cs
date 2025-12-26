using Microsoft.Data.SqlClient;

namespace TestProject1;

public class LocalDbFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = default!;

    private string _dbName = default!;
    private string _mdf = default!;
    private string _ldf = default!;

    public async Task InitializeAsync()
    {
        _dbName = $"TestDb_{Guid.NewGuid():N}";
        var baseDir = Path.Combine(Path.GetTempPath(), "LocalDbTests");
        Directory.CreateDirectory(baseDir);

        _mdf = Path.Combine(baseDir, $"{_dbName}.mdf");
        _ldf = Path.Combine(baseDir, $"{_dbName}_log.ldf");

        var masterConn = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;";

        using var conn = new SqlConnection(masterConn);
        await conn.OpenAsync();

        var createDbCmd = $@"
            CREATE DATABASE [{_dbName}]
            ON (NAME = N'{_dbName}', FILENAME = '{_mdf}')
            LOG ON (NAME = N'{_dbName}_log', FILENAME = '{_ldf}');
        ";

        using var cmd = new SqlCommand(createDbCmd, conn);
        await cmd.ExecuteNonQueryAsync();

        ConnectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={_dbName};Integrated Security=true;TrustServerCertificate=True;";
    }

    public static async Task RunScripts(string connectionString, string directory)
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        var files = Directory.GetFiles(directory, "*.sql").OrderBy(s => s);
        foreach (var file in files)
        {
            var script = File.ReadAllText(file)
                .Replace("\r", "")
                .Split("GO\n", StringSplitOptions.RemoveEmptyEntries);

            foreach (var batch in script)
            {
                using var cmd = new SqlCommand(batch, conn);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task DisposeAsync()
    {
        var masterConn = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;";
        using var conn = new SqlConnection(masterConn);
        await conn.OpenAsync();

        var killConnections = $@"
            DECLARE @kill VARCHAR(max)='';
            SELECT @kill = @kill + 'KILL ' + CONVERT(varchar(5), session_id) + ';'
            FROM sys.dm_exec_sessions WHERE database_id = DB_ID('{_dbName}');
            EXEC(@kill);
        ";
        using var killCmd = new SqlCommand(killConnections, conn);
        await killCmd.ExecuteNonQueryAsync();

        using var dropCmd = new SqlCommand($"DROP DATABASE IF EXISTS [{_dbName}];", conn);
        await dropCmd.ExecuteNonQueryAsync();

        TryDelete(_mdf);
        TryDelete(_ldf);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            /* swallow file locks */
        }
    }
}
