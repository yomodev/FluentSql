using FluentSqlLib;
using Microsoft.Data.SqlClient;
using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class SyncApiTests(LocalDbFixture db)
{
    [Fact]
    public void Query_MapsRowsSynchronously()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        conn.Open();
        using var insert = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('SyncQ', 'User', 'syncq@example.com');", conn);
        insert.ExecuteNonQuery();

        var fluent = db.CreateFluentSql();
        var users = fluent
            .Select("SELECT UserId, FirstName, LastName FROM dbo.Users WHERE LastName = @LastName")
            .WithParam("@LastName", "User")
            .Query<UserRow>()
            .ToList();

        users.Should().ContainSingle();
        users[0].FirstName.Should().Be("SyncQ");
    }

    [Fact]
    public void Get_ReturnsScalarSynchronously()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        conn.Open();
        using var insert = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('SyncG', 'Scalar', 'syncg@example.com');", conn);
        insert.ExecuteNonQuery();

        var fluent = db.CreateFluentSql();
        var firstName = fluent
            .Select("SELECT FirstName FROM dbo.Users WHERE LastName = @LastName")
            .WithParam("@LastName", "Scalar")
            .Get<string>("FirstName");

        firstName.Should().Be("SyncG");
    }

    [Fact]
    public void InsertMany_StreamsSynchronously()
    {
        var fluent = db.CreateFluentSql();
        var marker = Guid.NewGuid().ToString("N")[..8];
        var rows = Enumerable.Range(1, 20)
            .Select(i => new ProductRow { Name = $"{marker}-{i}", Price = i, Stock = i })
            .ToArray();

        var written = fluent.Table("dbo.Products").InsertMany(rows, batchSize: 5);

        written.Should().Be(20);

        var count = fluent
            .Select("SELECT COUNT(*) AS N FROM dbo.Products WHERE Name LIKE @M")
            .WithParam("@M", $"{marker}-%")
            .Get<int>("N");
        count.Should().Be(20);
    }

    [Fact]
    public void QueryMultiple_ReadsGridsSynchronously()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        conn.Open();
        using var seed = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('SyncMulti', 'X', 'syncmulti@example.com');", conn);
        seed.ExecuteNonQuery();

        var fluent = db.CreateFluentSql();
        using var results = fluent.StoredProcedure("dbo.sp_MultiResultTest").QueryMultiple();

        var users = results.ReadList<UserRow>();
        var orderCount = results.ReadScalar<int>();

        users.Should().NotBeEmpty();
        orderCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void StoredProcedure_Output_ReadSynchronously()
    {
        var fluent = db.CreateFluentSql();
        var outputs = fluent.StoredProcedure("dbo.sp_MixedOutputs")
            .WithParam("@inValue", 10)
            .WithOutputParam<int>("@outInt")
            .WithOutputParam<string>("@outString")
            .WithOutputParam<bool>("@outBit")
            .GetOutput();

        outputs["outInt"].Should().Be(20);
        outputs["outString"].Should().Be("echo-10");
    }

    private class UserRow
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    private class ProductRow
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
