using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class FluentFunctionContextTests(LocalDbFixture db)
{
    [Fact]
    public async Task AddTwoInts_Test()
    {
        // arrange
        var fluent = db.CreateFluentSql();
        var foo = fluent.Function("dbo.fn_AddTwoInts");

        // act
        var result = await foo
            .WithParam("a", 3)
            .WithParam("b", 5)
            .GetAsync<int>();

        // assert
        result.Should().Be(8);
    }

}
