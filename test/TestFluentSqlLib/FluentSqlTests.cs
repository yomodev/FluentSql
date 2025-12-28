using Microsoft.Data.SqlClient;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class FluentSqlTests(LocalDbFixture db)
{
    [Fact]
    public async Task ListDatabasesAsync()
    {
        var fluent = db.CreateFluentSql();
        var list = await fluent.ListDatabasesAsync().ToListAsync();
        list.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task CurrentDatabase()
    {
        var fluent = db.CreateFluentSql();
        var curDb = fluent.CurrentDatabase;
        curDb.Should().NotBeNull();
        curDb.Name.Should().Be(db.DatabaseName);
    }

    [Fact]
    public async Task CanReadUsers()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("SELECT COUNT(*) FROM Users;", conn);
        var count = (int)await cmd.ExecuteScalarAsync();

        Assert.True(count > 0);
    }
}
