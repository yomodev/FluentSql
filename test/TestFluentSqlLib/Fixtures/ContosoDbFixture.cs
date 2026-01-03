using FluentSqlLib;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace TestFluentSqlLib.Fixtures;

// download from
// https://github.com/sql-bi/Contoso-Data-Generator/releases/tag/v1.0.0
// https://red9.com/blog/sample-sql-databases/
public sealed class ContosoDbFixture : IDisposable
{
    public string DatabaseName { get; }
    public string ConnectionString { get; }
    private readonly string _dataFile;
    private readonly string _logFile;

    public ContosoDbFixture()
    {
        DatabaseName = "Contoso1M_Test";
        ConnectionString = $"Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Database={DatabaseName};";

        string localDbInstance = "(localdb)\\MSSQLLocalDB";
        string restorePath = Path.Combine(Path.GetTempPath(), DatabaseName);
        string bakFile = Path.GetFullPath(@"..\..\..\TestData\Contoso.1M.bak");
        if (!File.Exists(bakFile) )
        {
            throw new FileNotFoundException(string.Empty, bakFile);
        }

        Directory.CreateDirectory(restorePath);

        _dataFile = Path.Combine(restorePath, $"{DatabaseName}.mdf");
        _logFile = Path.Combine(restorePath, $"{DatabaseName}_log.ldf");

        if (!DatabaseExists())
            RestoreDatabase(localDbInstance, bakFile);
    }

    public FluentSql<FluentSqlSettings> CreateFluentSql()
    {
        var logger = NullLogger<FluentSqlSettings>.Instance;
        var settings = new FluentSqlSettings
        {
            ConnectionString = ConnectionString
        };

        return new FluentSql<FluentSqlSettings>(logger, settings);
    }

    private bool DatabaseExists()
    {
        using var cn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;");
        cn.Open();
        using var cmd = new SqlCommand($"SELECT db_id('{DatabaseName}')", cn);
        return cmd.ExecuteScalar() != DBNull.Value;
    }

    private void RestoreDatabase(string instance, string bakFile)
    {
        using var cn = new SqlConnection($"Server={instance};Integrated Security=true;");
        cn.Open();

        var server = new Server(new ServerConnection(cn));
        var restore = new Restore
        {
            Action = RestoreActionType.Database,
            Database = DatabaseName,
            ReplaceDatabase = true,
            NoRecovery = false
        };
        restore.Devices.AddDevice(bakFile, DeviceType.File);

        // Read logical names from .bak
        var logicalFiles = restore.ReadFileList(server);

        restore.RelocateFiles.Add(new RelocateFile(logicalFiles.Rows[0][0].ToString(), _dataFile));
        restore.RelocateFiles.Add(new RelocateFile(logicalFiles.Rows[1][0].ToString(), _logFile));

        restore.SqlRestore(server);
    }

    public void Dispose()
    {
        using var cn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;");
        cn.Open();

        using var cmd = new SqlCommand($@"
            ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE [{DatabaseName}];
        ", cn);

        cmd.ExecuteNonQuery();
    }
}
