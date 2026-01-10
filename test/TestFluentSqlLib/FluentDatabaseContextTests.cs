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

}
