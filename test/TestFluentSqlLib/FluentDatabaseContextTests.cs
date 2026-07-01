using Microsoft.Data.SqlClient;
using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class FluentDatabaseContextTests(LocalDbFixture db)
{
    [Fact]
    public async Task ListTablesAsync()
    {
        // arrange
        var fluent = db.CreateFluentSql();
        var curDb = fluent.CurrentDatabase;

        // act
        var list = await curDb.ListTablesAsync().ToListAsync();

        // assert
        list.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task ListFunctionsAsync()
    {
        // arrange
        var fluent = db.CreateFluentSql();
        var curDb = fluent.CurrentDatabase;

        // act
        var list = await curDb.ListFunctionsAsync().ToListAsync();

        // assert
        list.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task DropTableAsync_And_TruncateTableAsync_ExecuteValidSql()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var createCmd = new SqlCommand(
            "IF OBJECT_ID('dbo.DdlScratch', 'U') IS NOT NULL DROP TABLE dbo.DdlScratch; CREATE TABLE dbo.DdlScratch (Id INT);", conn);
        await createCmd.ExecuteNonQueryAsync();
        using var insertCmd = new SqlCommand("INSERT INTO dbo.DdlScratch (Id) VALUES (1);", conn);
        await insertCmd.ExecuteNonQueryAsync();

        var fluent = db.CreateFluentSql();
        var curDb = fluent.CurrentDatabase;

        var truncated = await curDb.TruncateTableAsync("dbo.DdlScratch");
        truncated.Should().BeTrue(); // must execute without a syntax error

        using var countCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.DdlScratch;", conn);
        var count = (int)await countCmd.ExecuteScalarAsync();
        count.Should().Be(0);

        var dropped = await curDb.DropTableAsync("dbo.DdlScratch");
        dropped.Should().BeTrue(); // must execute without a syntax error

        using var checkCmd = new SqlCommand("SELECT OBJECT_ID('dbo.DdlScratch', 'U');", conn);
        var objectId = await checkCmd.ExecuteScalarAsync();
        objectId.Should().Be(DBNull.Value);
    }
}
