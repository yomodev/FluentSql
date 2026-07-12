using FluentSqlLib;
using Microsoft.Data.SqlClient;
using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class InsertManyAsyncTests(LocalDbFixture db)
{
    [Fact]
    public async Task InsertManyAsync_StreamsRowsAndExcludesIdentityColumn()
    {
        var fluent = db.CreateFluentSql();
        // Marker-scoped so the assertion is robust against the shared Products table.
        var marker = Guid.NewGuid().ToString("N")[..8];
        var rows = Enumerable.Range(1, 50)
            .Select(i => new ProductRow { Id = -1, Name = $"{marker}-{i}", Price = i * 1.5m, Stock = i })
            .ToArray();

        // batchSize smaller than the row count exercises SqlBulkCopy actually batching internally.
        var written = await fluent.Table("dbo.Products").InsertManyAsync(rows, batchSize: 10);

        written.Should().Be(50);

        using var conn = new SqlConnection(db.ConnectionString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT COUNT(*), MIN(ProductId) FROM dbo.Products WHERE Name LIKE @M;", conn);
        cmd.Parameters.AddWithValue("@M", $"{marker}-%");
        using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        reader.GetInt32(0).Should().Be(50);
        // ProductId is IDENTITY and Id is marked [SqlColumn(Identity = true)] with a bogus
        // value (-1); if it weren't excluded from the bulk copy, this insert would fail
        // outright (IDENTITY_INSERT is off) rather than silently succeed with -1 values.
        reader.GetInt32(1).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task InsertManyAsync_RoundTripsThroughRegisteredConverter()
    {
        ColumnConverterRegistry.Register<Quantity, int>(q => q.Value, v => new Quantity(v));
        try
        {
            var fluent = db.CreateFluentSql();
            var marker = Guid.NewGuid().ToString("N")[..8];
            var rows = new[]
            {
                new ProductWithQuantityRow { Id = -1, Name = marker, Price = 9.99m, Stock = new Quantity(42) },
            };

            await fluent.Table("dbo.Products").InsertManyAsync(rows);

            var result = await fluent
                .Select("SELECT ProductId, Name, Price, Stock FROM dbo.Products WHERE Name = @Name")
                .WithParam("@Name", marker)
                .QueryAsync<ProductWithQuantityRow>()
                .ToListAsync();

            result.Should().ContainSingle();
            result[0].Stock.Value.Should().Be(42);
        }
        finally
        {
            ColumnConverterRegistry.Unregister<Quantity>();
        }
    }

    private class ProductRow
    {
        [SqlColumn(Name = "ProductId", Identity = true)]
        public int Id { get; set; }

        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    private readonly struct Quantity(int value)
    {
        public int Value { get; } = value;
    }

    private class ProductWithQuantityRow
    {
        [SqlColumn(Name = "ProductId", Identity = true)]
        public int Id { get; set; }

        public string? Name { get; set; }
        public decimal Price { get; set; }
        public Quantity Stock { get; set; }
    }
}
