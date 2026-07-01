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
}
