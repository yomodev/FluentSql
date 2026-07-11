using Microsoft.Data.SqlClient;
using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class MultipleResultsTests(LocalDbFixture db)
{
    [Fact]
    public async Task QueryMultipleAsync_ReadsGridsInOrder()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using (var seed = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('Multi', 'One', 'm1@example.com'), ('Multi', 'Two', 'm2@example.com');", conn))
        {
            await seed.ExecuteNonQueryAsync();
        }

        var fluent = db.CreateFluentSql();

        // sp_MultiResultTest: grid 1 = SELECT TOP 2 * FROM Users, grid 2 = SELECT COUNT(*) FROM Orders
        await using var results = await fluent.StoredProcedure("dbo.sp_MultiResultTest").QueryMultipleAsync();

        var users = await results.ReadListAsync<UserRow>();
        var orderCount = await results.ReadScalarAsync<int>();

        users.Should().HaveCount(2);
        users.Should().OnlyContain(u => u.UserId > 0);
        orderCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task QueryMultipleAsync_ReadingPastLastGrid_Throws()
    {
        var fluent = db.CreateFluentSql();
        await using var results = await fluent.StoredProcedure("dbo.sp_MultiResultTest").QueryMultipleAsync();

        await results.ReadListAsync<UserRow>();   // grid 1
        await results.ReadScalarAsync<int>();     // grid 2 (last)

        // No third grid.
        var act = async () => await results.ReadScalarAsync<int>();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task QueryMultipleAsync_StreamsFirstGridThenAdvances()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using (var seed = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('Stream', 'A', 'sa@example.com');", conn))
        {
            await seed.ExecuteNonQueryAsync();
        }

        var fluent = db.CreateFluentSql();
        await using var results = await fluent.StoredProcedure("dbo.sp_MultiResultTest").QueryMultipleAsync();

        var streamed = new List<UserRow>();
        await foreach (var user in results.ReadAsync<UserRow>())
        {
            streamed.Add(user);
        }

        var orderCount = await results.ReadScalarAsync<int>();

        streamed.Should().NotBeEmpty();
        orderCount.Should().BeGreaterThanOrEqualTo(0);
    }

    private class UserRow
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
