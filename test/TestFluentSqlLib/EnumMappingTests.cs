using Microsoft.Data.SqlClient;
using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class EnumMappingTests(LocalDbFixture db)
{
    private enum Availability
    {
        None = 0,
        Low = 5,
        High = 100,
    }

    [Fact]
    public async Task Enum_RoundTripsThroughBulkInsertAndReader()
    {
        var fluent = db.CreateFluentSql();
        var marker = Guid.NewGuid().ToString("N")[..8];
        var rows = new[]
        {
            new ProductWithEnum { Name = $"{marker}-a", Price = 1m, Stock = Availability.High },
            new ProductWithEnum { Name = $"{marker}-b", Price = 2m, Stock = Availability.Low },
        };

        await fluent.Table("dbo.Products").InsertManyAsync(rows);

        // Verify the raw stored value is the numeric underlying (100), not something enum-shaped.
        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT Stock FROM dbo.Products WHERE Name = @Name;", conn);
        cmd.Parameters.AddWithValue("@Name", $"{marker}-a");
        ((int)(await cmd.ExecuteScalarAsync())!).Should().Be(100);

        var read = await fluent
            .Select("SELECT Name, Price, Stock FROM dbo.Products WHERE Name LIKE @Marker ORDER BY Name")
            .WithParam("@Marker", $"{marker}-%")
            .QueryAsync<ProductWithEnum>()
            .ToListAsync();

        read.Should().HaveCount(2);
        read[0].Stock.Should().Be(Availability.High);
        read[1].Stock.Should().Be(Availability.Low);
    }

    [Fact]
    public async Task NullableEnum_RoundTripsNullAndValue()
    {
        var fluent = db.CreateFluentSql();
        var marker = Guid.NewGuid().ToString("N")[..8];
        var rows = new[]
        {
            new ProductWithNullableEnum { Name = $"{marker}-set", Price = 1m, Stock = Availability.Low },
            new ProductWithNullableEnum { Name = $"{marker}-null", Price = 2m, Stock = null },
        };

        await fluent.Table("dbo.Products").InsertManyAsync(rows);

        var read = await fluent
            .Select("SELECT Name, Price, Stock FROM dbo.Products WHERE Name LIKE @Marker ORDER BY Name")
            .WithParam("@Marker", $"{marker}-%")
            .QueryAsync<ProductWithNullableEnum>()
            .ToListAsync();

        read.Should().HaveCount(2);
        // "-null" sorts before "-set"
        read[0].Stock.Should().BeNull();
        read[1].Stock.Should().Be(Availability.Low);
    }

    private class ProductWithEnum
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public Availability Stock { get; set; }
    }

    private class ProductWithNullableEnum
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public Availability? Stock { get; set; }
    }
}
