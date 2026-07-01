using Microsoft.Data.SqlClient;
using TestFluentSqlLib.Fixtures;

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

        using var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users;", conn);
        var count = (int)await cmd.ExecuteScalarAsync();

        Assert.True(count > -1);
    }

    [Fact]
    public async Task Select_MapsRawQueryToDto()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('Ada', 'Lovelace', 'ada@example.com');", conn);
        await insertCmd.ExecuteNonQueryAsync();

        var fluent = db.CreateFluentSql();
        var users = await fluent
            .Select("SELECT UserId, FirstName, LastName, Email, CreatedAt FROM dbo.Users WHERE LastName = @LastName")
            .WithParam("@LastName", "Lovelace")
            .QueryAsync<UserDto>()
            .ToListAsync();

        users.Should().ContainSingle();
        users[0].FirstName.Should().Be("Ada");
        users[0].LastName.Should().Be("Lovelace");
        users[0].Email.Should().Be("ada@example.com");
        users[0].CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task Select_GetAsyncColumn_ReturnsScalarValue()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('Grace', 'Hopper', 'grace@example.com');", conn);
        await insertCmd.ExecuteNonQueryAsync();

        var fluent = db.CreateFluentSql();
        var firstName = await fluent
            .Select("SELECT FirstName FROM dbo.Users WHERE LastName = @LastName")
            .WithParam("@LastName", "Hopper")
            .GetAsync<string>("FirstName");

        firstName.Should().Be("Grace");
    }

    [Fact]
    public async Task Select_GetAsyncColumn_ReturnsNullWhenNoRows()
    {
        var fluent = db.CreateFluentSql();
        var result = await fluent
            .Select("SELECT FirstName FROM dbo.Users WHERE LastName = @LastName")
            .WithParam("@LastName", "NoSuchLastName")
            .GetAsync<string>("FirstName");

        result.Should().BeNull();
    }

    [Fact]
    public async Task StoredProcedure_TableValuedParameter_BulkInsertsOrders()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertUserCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) OUTPUT INSERTED.UserId VALUES ('Tvp', 'Test', 'tvp@example.com');", conn);
        var userId = (int)(await insertUserCmd.ExecuteScalarAsync())!;

        var fluent = db.CreateFluentSql();
        var orders = new[]
        {
            new OrderRow { UserId = userId, Total = 10.50m },
            new OrderRow { UserId = userId, Total = 20.00m },
        };

        await fluent.StoredProcedure("dbo.sp_BulkInsertOrders")
            .WithParam("@orders", orders, "dbo.OrderTableType")
            .GetAsync<int>();

        using var countCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Orders WHERE UserId = @UserId;", conn);
        countCmd.Parameters.AddWithValue("@UserId", userId);
        var count = (int)(await countCmd.ExecuteScalarAsync())!;

        count.Should().Be(2);
    }

    private class UserDto
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private class OrderRow
    {
        public int UserId { get; set; }
        public decimal Total { get; set; }
    }
}
