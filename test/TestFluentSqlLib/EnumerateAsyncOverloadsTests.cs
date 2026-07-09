using System.Data;
using Microsoft.Data.SqlClient;
using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class EnumerateAsyncOverloadsTests(LocalDbFixture db)
{
    [Fact]
    public async Task QueryAsync_PropertyToColumnDictionary_IsFullReplacement()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('Dorothy', 'Vaughan', 'dorothy@example.com');", conn);
        await insertCmd.ExecuteNonQueryAsync();

        var fluent = db.CreateFluentSql();
        var map = new Dictionary<string, string>
        {
            ["FullFirst"] = "FirstName",
            ["FullLast"] = "LastName",
        };

        var people = await fluent
            .Select("SELECT FirstName, LastName, Email FROM dbo.Users WHERE LastName = @LastName")
            .WithParam("@LastName", "Vaughan")
            .QueryAsync<PlainPersonDto>(map)
            .ToListAsync();

        people.Should().ContainSingle();
        people[0].FullFirst.Should().Be("Dorothy");
        people[0].FullLast.Should().Be("Vaughan");
        // Email is not in the dictionary, so it must stay unset even though the column is present.
        people[0].Email.Should().BeNull();
    }

    [Fact]
    public async Task QueryAsync_ColumnSetterDictionary_IsFullReplacement()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('Mary', 'Jackson', 'mary@example.com');", conn);
        await insertCmd.ExecuteNonQueryAsync();

        var fluent = db.CreateFluentSql();
        var setters = new Dictionary<string, Action<PlainPersonDto, IDataRecord>>
        {
            ["FirstName"] = (t, r) => t.FullFirst = r.GetString(r.GetOrdinal("FirstName")).ToUpperInvariant(),
            ["LastName"] = (t, r) => t.FullLast = r.GetString(r.GetOrdinal("LastName")),
        };

        var people = await fluent
            .Select("SELECT FirstName, LastName, Email FROM dbo.Users WHERE LastName = @LastName")
            .WithParam("@LastName", "Jackson")
            .QueryAsync<PlainPersonDto>(setters)
            .ToListAsync();

        people.Should().ContainSingle();
        people[0].FullFirst.Should().Be("MARY");
        people[0].FullLast.Should().Be("Jackson");
        people[0].Email.Should().BeNull();
    }

    [Fact]
    public async Task QueryAsync_FuncMapper_BypassesColumnResolutionEntirely()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insertCmd = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('Christine', 'Darden', 'christine@example.com');", conn);
        await insertCmd.ExecuteNonQueryAsync();

        var fluent = db.CreateFluentSql();
        var people = await fluent
            .Select("SELECT FirstName, LastName FROM dbo.Users WHERE LastName = @LastName")
            .WithParam("@LastName", "Darden")
            .QueryAsync(r => new PlainPersonDto
            {
                FullFirst = r.GetString(0),
                FullLast = r.GetString(1),
            })
            .ToListAsync();

        people.Should().ContainSingle();
        people[0].FullFirst.Should().Be("Christine");
        people[0].FullLast.Should().Be("Darden");
    }

    [Fact]
    public async Task QueryAsync_SkipMissingColumnsTrue_LeavesUnmatchedPropertyDefault()
    {
        var fluent = db.CreateFluentSql();
        var result = await fluent
            .Select("SELECT FirstName FROM dbo.Users")
            .QueryAsync<UserWithPhoneDto>()
            .ToListAsync();

        result.Should().NotBeEmpty();
        result[0].Phone.Should().BeNull();
    }

    [Fact]
    public async Task QueryAsync_SkipMissingColumnsFalse_ThrowsWhenColumnMissing()
    {
        var fluent = db.CreateFluentSql();

        var act = async () => await fluent
            .Select("SELECT FirstName FROM dbo.Users")
            .QueryAsync<UserWithPhoneDto>(skipMissingColumns: false)
            .ToListAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private class PlainPersonDto
    {
        public string? FullFirst { get; set; }
        public string? FullLast { get; set; }
        public string? Email { get; set; }
    }

    private class UserWithPhoneDto
    {
        public string? FirstName { get; set; }
        public string? Phone { get; set; }
    }
}
