using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

// download from
// https://github.com/sql-bi/Contoso-Data-Generator/releases/tag/v1.0.0
// https://red9.com/blog/sample-sql-databases/
public class ContosoTests(ContosoDbFixture contoso) : IClassFixture<ContosoDbFixture>
{
    [Fact]
    public async Task CanQueryProducts()
    {
        // arrange
        var fluent = contoso.CreateFluentSql();
        var curDb = fluent.CurrentDatabase;

        // act
        var list = await curDb.ListTablesAsync().ToListAsync();

        // assert
        list.Should().HaveCountGreaterThan(0);
        /*
        using var cn = new SqlConnection(_cs);
        cn.Open();

        using var cmd = new SqlCommand("SELECT * FROM Data.Store", cn);
        using var reader = cmd.ExecuteReader();

        Assert.True(reader.HasRows);*/
    }
}
