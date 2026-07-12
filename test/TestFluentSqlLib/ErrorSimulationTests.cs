using FluentSqlLib;
using Microsoft.Data.SqlClient;
using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class ErrorSimulationTests(LocalDbFixture db)
{
    [Fact]
    public async Task TypeMismatch_ThrowsMappingExceptionNamingColumnValueAndProperty()
    {
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var insert = new SqlCommand(
            "INSERT INTO dbo.Users (FirstName, LastName, Email) VALUES ('NotANumber', 'Boom', 'boom@example.com');", conn);
        await insert.ExecuteNonQueryAsync();

        var fluent = db.CreateFluentSql();

        // FirstName is nvarchar; the DTO wrongly types it as int, forcing a conversion failure.
        var act = async () => await fluent
            .Select("SELECT FirstName FROM dbo.Users WHERE LastName = @LastName")
            .WithParam("@LastName", "Boom")
            .QueryAsync<BadTypeDto>()
            .ToListAsync();

        var ex = (await act.Should().ThrowAsync<FluentSqlMappingException>()).Which;
        ex.ColumnName.Should().Be("FirstName");
        ex.PropertyName.Should().Be("FirstName");
        ex.TargetType.Should().Be(typeof(int));
        ex.Message.Should().Contain("FirstName");
        // The source DB type is named even when the value can't be re-read under sequential access.
        ex.Message.Should().Contain("Int32");
    }

    [Fact]
    public async Task MissingColumn_SkipFalse_ThrowsNamingColumnAndProperty()
    {
        var fluent = db.CreateFluentSql();

        var act = async () => await fluent
            .Select("SELECT FirstName FROM dbo.Users")
            .QueryAsync<UserWithMissingColumnDto>(skipMissingColumns: false)
            .ToListAsync();

        var ex = (await act.Should().ThrowAsync<InvalidOperationException>()).Which;
        ex.Message.Should().Contain("Missing");        // the column name
        ex.Message.Should().Contain("PhoneNumber");    // the property name
    }

    [Fact]
    public async Task PropertyDictionary_UnknownProperty_ThrowsNamingIt()
    {
        var fluent = db.CreateFluentSql();
        var map = new Dictionary<string, string> { ["DoesNotExist"] = "FirstName" };

        var act = async () => await fluent
            .Select("SELECT FirstName FROM dbo.Users")
            .QueryAsync<BadTypeDto>(map)
            .ToListAsync();

        var ex = (await act.Should().ThrowAsync<InvalidOperationException>()).Which;
        ex.Message.Should().Contain("DoesNotExist");
    }

    [Fact]
    public async Task ColumnSetterDictionary_MissingColumn_SkipFalse_ThrowsNamingColumn()
    {
        var fluent = db.CreateFluentSql();
        var setters = new Dictionary<string, Action<BadTypeDto, System.Data.IDataRecord>>
        {
            ["NoSuchColumn"] = (_, _) => { },
        };

        var act = async () => await fluent
            .Select("SELECT FirstName FROM dbo.Users")
            .QueryAsync<BadTypeDto>(setters, skipMissingColumns: false)
            .ToListAsync();

        var ex = (await act.Should().ThrowAsync<InvalidOperationException>()).Which;
        ex.Message.Should().Contain("NoSuchColumn");
    }

    private class BadTypeDto
    {
        public int FirstName { get; set; }
    }

    private class UserWithMissingColumnDto
    {
        public string? FirstName { get; set; }

        [SqlColumn(Name = "Missing_PhoneNumber")]
        public string? PhoneNumber { get; set; }
    }
}
