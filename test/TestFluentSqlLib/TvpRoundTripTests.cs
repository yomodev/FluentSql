using FluentSqlLib;
using TestFluentSqlLib.Fixtures;

namespace TestFluentSqlLib;

[Collection("SharedLocalDb")]
public class TvpRoundTripTests(LocalDbFixture db)
{
    [Fact]
    public async Task WideTvp_RoundTripsAllTypesAndNulls()
    {
        var fluent = db.CreateFluentSql();
        var g1 = Guid.NewGuid();
        var g2 = Guid.NewGuid();
        var date = new DateTime(2026, 7, 12, 8, 30, 0);

        var rows = new[]
        {
            new WideRow
            {
                IntCol = 1, BitCol = true, DecimalCol = 12.3456m, FloatCol = 1.5,
                GuidCol = g1, DateCol = date, TextCol = "first", NullableIntCol = 99,
            },
            new WideRow
            {
                IntCol = 2, BitCol = false, DecimalCol = 0.0001m, FloatCol = -2.25,
                GuidCol = g2, DateCol = date.AddDays(1), TextCol = null, NullableIntCol = null,
            },
        };

        var result = await fluent.StoredProcedure("dbo.sp_EchoWideTvp")
            .WithParam("@rows", rows, "dbo.WideTableType")
            .EnumerateAsync<WideRow>()
            .ToListAsync();

        result.Should().HaveCount(2);

        result[0].IntCol.Should().Be(1);
        result[0].BitCol.Should().BeTrue();
        result[0].DecimalCol.Should().Be(12.3456m);
        result[0].FloatCol.Should().Be(1.5);
        result[0].GuidCol.Should().Be(g1);
        result[0].DateCol.Should().Be(date);
        result[0].TextCol.Should().Be("first");
        result[0].NullableIntCol.Should().Be(99);

        result[1].BitCol.Should().BeFalse();
        result[1].TextCol.Should().BeNull();
        result[1].NullableIntCol.Should().BeNull();
    }

    [Fact]
    public async Task Tvp_IncludesIdentityColumn_UnlikeInsert()
    {
        // A DTO carrying [SqlColumn(Identity = true)] on IntCol must still send that column
        // through a TVP (identity/computed exclusion is insert-only), so the echo round-trips it.
        var fluent = db.CreateFluentSql();
        var rows = new[]
        {
            new IdentityCarryingRow { IntCol = 7, BitCol = true },
        };

        var result = await fluent.StoredProcedure("dbo.sp_EchoWideTvp")
            .WithParam("@rows", rows, "dbo.WideTableType")
            .EnumerateAsync<WideRow>()
            .ToListAsync();

        result.Should().ContainSingle();
        result[0].IntCol.Should().Be(7);
    }

    private class WideRow
    {
        public int IntCol { get; set; }
        public bool BitCol { get; set; }
        public decimal DecimalCol { get; set; }
        public double FloatCol { get; set; }
        public Guid GuidCol { get; set; }
        public DateTime DateCol { get; set; }
        public string? TextCol { get; set; }
        public int? NullableIntCol { get; set; }
    }

    private class IdentityCarryingRow
    {
        [SqlColumn(Identity = true, Ordinal = 0)]
        public int IntCol { get; set; }

        [SqlColumn(Ordinal = 1)]
        public bool BitCol { get; set; }

        [SqlColumn(Ordinal = 2)]
        public decimal DecimalCol { get; set; }

        [SqlColumn(Ordinal = 3)]
        public double FloatCol { get; set; }

        [SqlColumn(Ordinal = 4)]
        public Guid GuidCol { get; set; }

        [SqlColumn(Ordinal = 5)]
        public DateTime DateCol { get; set; }

        [SqlColumn(Ordinal = 6)]
        public string? TextCol { get; set; }

        [SqlColumn(Ordinal = 7)]
        public int? NullableIntCol { get; set; }
    }
}
