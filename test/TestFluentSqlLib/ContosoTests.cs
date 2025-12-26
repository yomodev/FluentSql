using Microsoft.Data.SqlClient;

namespace TestProject1;

// download from
// https://github.com/sql-bi/Contoso-Data-Generator/releases/tag/v1.0.0
// https://red9.com/blog/sample-sql-databases/
public class ContosoTests : IClassFixture<ContosoLocalDbFixture>
{
    private readonly string _cs;

    public ContosoTests(ContosoLocalDbFixture fixture)
    {
        _cs = fixture.ConnectionString;
    }

    [Fact]
    public void CanQueryProducts()
    {
        using var cn = new SqlConnection(_cs);
        cn.Open();

        using var cmd = new SqlCommand("SELECT * FROM Data.Store", cn);
        using var reader = cmd.ExecuteReader();

        Assert.True(reader.HasRows);
    }
}
