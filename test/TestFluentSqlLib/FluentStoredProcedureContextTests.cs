using Microsoft.Data.SqlClient;
using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class FluentStoredProcedureContextTests(LocalDbFixture db)
{
    [Fact]
    public async Task WithOutputParam_ReturnsOutputValue()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertUserCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) OUTPUT INSERTED.UserId VALUES ('Output', 'Test', 'output@example.com');", conn);
        var userId = (int)(await insertUserCmd.ExecuteScalarAsync())!;

        using var insertOrderCmd = new SqlCommand(
            "INSERT INTO dbo.Orders (UserId, Total) OUTPUT INSERTED.OrderId VALUES (@UserId, 42.50);", conn);
        insertOrderCmd.Parameters.AddWithValue("@UserId", userId);
        var orderId = (int)(await insertOrderCmd.ExecuteScalarAsync())!;

        var fluent = db.CreateFluentSql();
        var total = await fluent.StoredProcedure("dbo.sp_GetOrderTotal")
            .WithParam("@orderId", orderId)
            .WithOutputParam<decimal>("@total", precision: 10, scale: 2)
            .GetOutputAsync<decimal>("total");

        total.Should().Be(42.50m);
    }

    [Fact]
    public async Task WithOutputParam_WithoutPrecisionScale_TruncatesDecimal()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertUserCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) OUTPUT INSERTED.UserId VALUES ('OutputTrunc', 'Test', 'outputtrunc@example.com');", conn);
        var userId = (int)(await insertUserCmd.ExecuteScalarAsync())!;

        using var insertOrderCmd = new SqlCommand(
            "INSERT INTO dbo.Orders (UserId, Total) OUTPUT INSERTED.OrderId VALUES (@UserId, 42.50);", conn);
        insertOrderCmd.Parameters.AddWithValue("@UserId", userId);
        var orderId = (int)(await insertOrderCmd.ExecuteScalarAsync())!;

        var fluent = db.CreateFluentSql();
        var total = await fluent.StoredProcedure("dbo.sp_GetOrderTotal")
            .WithParam("@orderId", orderId)
            .WithOutputParam<decimal>("@total")
            .GetOutputAsync<decimal>("total");

        // Without explicit precision/scale, the output parameter defaults to scale 0.
        total.Should().Be(43m);
    }

    [Fact]
    public async Task MixedTypeOutputParameters_AreReadBackByName()
    {
        var fluent = db.CreateFluentSql();
        var outputs = await fluent.StoredProcedure("dbo.sp_MixedOutputs")
            .WithParam("@inValue", 21)
            .WithOutputParam<int>("@outInt")
            .WithOutputParam<string>("@outString")
            .WithOutputParam<bool>("@outBit")
            .GetOutputAsync();

        outputs["outInt"].Should().Be(42);
        outputs["outString"].Should().Be("echo-21");
        outputs["outBit"].Should().Be(true);
    }

    [Fact]
    public async Task StoredProcedure_ReturningRows_MapsToDto()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertUserCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) OUTPUT INSERTED.UserId VALUES ('Proc', 'Row', 'procrow@example.com');", conn);
        var userId = (int)(await insertUserCmd.ExecuteScalarAsync())!;

        var fluent = db.CreateFluentSql();
        var users = await fluent.StoredProcedure("dbo.sp_GetUserById")
            .WithParam("@id", userId)
            .EnumerateAsync<UserRow>()
            .ToListAsync();

        users.Should().ContainSingle();
        users[0].UserId.Should().Be(userId);
        users[0].FirstName.Should().Be("Proc");
        users[0].LastName.Should().Be("Row");
    }

    [Fact]
    public async Task InsertOrderWithOutputId_ReturnsGeneratedId()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertUserCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) OUTPUT INSERTED.UserId VALUES ('Outid', 'Test', 'outid@example.com');", conn);
        var userId = (int)(await insertUserCmd.ExecuteScalarAsync())!;

        var fluent = db.CreateFluentSql();
        var newId = await fluent.StoredProcedure("dbo.sp_InsertOrderWithOutputId")
            .WithParam("@userId", userId)
            .WithParam("@total", 15.00m)
            .WithOutputParam<int>("@newId")
            .GetOutputAsync<int>("newId");

        newId.Should().BeGreaterThan(0);
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
