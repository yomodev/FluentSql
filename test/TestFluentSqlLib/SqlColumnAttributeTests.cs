using FluentSqlLib;
using Microsoft.Data.SqlClient;
using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class SqlColumnAttributeTests(LocalDbFixture db)
{
    [Fact]
    public async Task QueryAsync_HonorsNameOverrideAndIgnore()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('Katherine', 'Johnson', 'katherine@example.com');", conn);
        await insertCmd.ExecuteNonQueryAsync();

        var fluent = db.CreateFluentSql();
        var people = await fluent
            .Select("SELECT FirstName, LastName, Email FROM dbo.Users WHERE LastName = @LastName")
            .WithParam("@LastName", "Johnson")
            .QueryAsync<NamedColumnDto>()
            .ToListAsync();

        people.Should().ContainSingle();
        people[0].GivenName.Should().Be("Katherine");
        people[0].Surname.Should().Be("Johnson");
        // Ignored despite the Email column being present in the result set.
        people[0].Email.Should().BeNull();
    }

    [Fact]
    public async Task QueryAsync_HonorsExplicitOrdinal()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('Margaret', 'Hamilton', 'margaret@example.com');", conn);
        await insertCmd.ExecuteNonQueryAsync();

        var fluent = db.CreateFluentSql();
        var rows = await fluent
            .Select("SELECT FirstName, LastName FROM dbo.Users WHERE LastName = @LastName")
            .WithParam("@LastName", "Hamilton")
            .QueryAsync<OrdinalDto>()
            .ToListAsync();

        rows.Should().ContainSingle();
        rows[0].First.Should().Be("Margaret");
        rows[0].Second.Should().Be("Hamilton");
    }

    [Fact]
    public async Task TableValuedParameter_HonorsNameAndOrdinalOverrides()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertUserCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) OUTPUT INSERTED.UserId VALUES ('ColumnAttr', 'Test', 'columnattr@example.com');", conn);
        var userId = (int)(await insertUserCmd.ExecuteScalarAsync())!;

        var fluent = db.CreateFluentSql();
        // Properties are declared out of order relative to dbo.OrderTableType (UserId, Total);
        // explicit Ordinal must be what determines the DataTable column order, not declaration order.
        var rows = new[]
        {
            new ReorderedOrderRow { Amount = 15.25m, Uid = userId },
        };

        await fluent.StoredProcedure("dbo.sp_BulkInsertOrders")
            .WithParam("@orders", rows, "dbo.OrderTableType")
            .GetAsync<int>();

        using var checkCmd = new SqlCommand("SELECT Total FROM dbo.Orders WHERE UserId = @UserId;", conn);
        checkCmd.Parameters.AddWithValue("@UserId", userId);
        var total = (decimal)(await checkCmd.ExecuteScalarAsync())!;

        total.Should().Be(15.25m);
    }

    [Fact]
    public void SqlParameterFactory_ExcludesComputedAndIdentityColumns()
    {
        var poco = new ParamDto { Id = 1, Name = "test", RowVersion = [1, 2, 3] };

        var parameters = Mapper.SqlParameterFactory.CreateParameters(poco).ToList();

        parameters.Select(p => p.ParameterName).Should().BeEquivalentTo(["@Name"]);
    }

    private class NamedColumnDto
    {
        [SqlColumn(Name = "FirstName")]
        public string? GivenName { get; set; }

        [SqlColumn(Name = "LastName")]
        public string? Surname { get; set; }

        [SqlColumn(Ignore = true)]
        public string? Email { get; set; }
    }

    private class OrdinalDto
    {
        [SqlColumn(Ordinal = 0)]
        public string? First { get; set; }

        [SqlColumn(Ordinal = 1)]
        public string? Second { get; set; }
    }

    private class ReorderedOrderRow
    {
        [SqlColumn(Name = "Total", Ordinal = 1)]
        public decimal Amount { get; set; }

        [SqlColumn(Name = "UserId", Ordinal = 0)]
        public int Uid { get; set; }
    }

    private class ParamDto
    {
        [SqlColumn(Identity = true)]
        public int Id { get; set; }

        public string? Name { get; set; }

        [SqlColumn(Computed = true)]
        public byte[]? RowVersion { get; set; }
    }
}
