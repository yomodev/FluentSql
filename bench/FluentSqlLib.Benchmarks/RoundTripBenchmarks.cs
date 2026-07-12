using BenchmarkDotNet.Attributes;

namespace FluentSqlLib.Benchmarks;

public enum Status
{
    None = 0,
    Active = 1,
    Archived = 2,
}

public readonly struct Quantity(int value)
{
    public int Value { get; } = value;
}

/// <summary>
/// A wide DTO exercising as many SQL types as practical, plus bool/bit, an enum stored as int,
/// and a custom converter type (Quantity).
/// </summary>
public sealed class WideRow
{
    [SqlColumn(Identity = true)]
    public int Id { get; set; }

    public int IntCol { get; set; }
    public long BigIntCol { get; set; }
    public bool BitCol { get; set; }
    public decimal DecimalCol { get; set; }
    public double FloatCol { get; set; }
    public float RealCol { get; set; }
    public Guid GuidCol { get; set; }
    public DateTime DateCol { get; set; }
    public DateTimeOffset OffsetCol { get; set; }
    public string? TextCol { get; set; }
    public Status StatusCol { get; set; }
    public Quantity QuantityCol { get; set; }
    public int? NullableIntCol { get; set; }
}

/// <summary>
/// Round-trip benchmark: generate <see cref="RowCount"/> rows on the fly, bulk-insert them with
/// InsertMany (streaming, no DataTable), then read them all back through the compiled mapper.
/// Compares bulk insert against naive row-by-row insert, and measures the read/map path.
/// </summary>
[MemoryDiagnoser]
public class RoundTripBenchmarks
{
    [Params(1_000, 10_000)]
    public int RowCount { get; set; }

    private BenchmarkDb _db = null!;
    private FluentSql<FluentSqlSettings> _fluent = null!;
    private WideRow[] _rows = null!;

    [GlobalSetup]
    public void Setup()
    {
        ColumnConverterRegistry.Register<Quantity, int>(q => q.Value, v => new Quantity(v));
        _db = new BenchmarkDb();
        _fluent = _db.CreateFluent();
        _rows = Generate(RowCount).ToArray();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        ColumnConverterRegistry.Unregister<Quantity>();
        _db.Dispose();
    }

    [IterationSetup]
    public void IterationSetup() => _db.Truncate();

    [Benchmark(Baseline = true)]
    public int RowByRowInsert()
    {
        using var conn = new SqlConnection(_db.ConnectionString);
        conn.Open();
        var n = 0;
        foreach (var r in _rows)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO dbo.WideRows (IntCol,BigIntCol,BitCol,DecimalCol,FloatCol,RealCol,GuidCol,DateCol,OffsetCol,TextCol,StatusCol,QuantityCol,NullableIntCol)
                  VALUES (@i,@b,@bit,@d,@f,@r,@g,@dt,@o,@t,@s,@q,@n)", conn);
            cmd.Parameters.AddWithValue("@i", r.IntCol);
            cmd.Parameters.AddWithValue("@b", r.BigIntCol);
            cmd.Parameters.AddWithValue("@bit", r.BitCol);
            cmd.Parameters.AddWithValue("@d", r.DecimalCol);
            cmd.Parameters.AddWithValue("@f", r.FloatCol);
            cmd.Parameters.AddWithValue("@r", r.RealCol);
            cmd.Parameters.AddWithValue("@g", r.GuidCol);
            cmd.Parameters.AddWithValue("@dt", r.DateCol);
            cmd.Parameters.AddWithValue("@o", r.OffsetCol);
            cmd.Parameters.AddWithValue("@t", (object?)r.TextCol ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@s", (int)r.StatusCol);
            cmd.Parameters.AddWithValue("@q", r.QuantityCol.Value);
            cmd.Parameters.AddWithValue("@n", (object?)r.NullableIntCol ?? DBNull.Value);
            n += cmd.ExecuteNonQuery();
        }

        return n;
    }

    [Benchmark]
    public long BulkInsert()
        => _fluent.Table("dbo.WideRows").InsertMany(_rows);

    [Benchmark]
    public int BulkInsertThenReadBack()
    {
        _fluent.Table("dbo.WideRows").InsertMany(_rows);
        var read = _fluent
            .Select("SELECT IntCol,BigIntCol,BitCol,DecimalCol,FloatCol,RealCol,GuidCol,DateCol,OffsetCol,TextCol,StatusCol,QuantityCol,NullableIntCol FROM dbo.WideRows")
            .Query<WideRow>()
            .Count();
        return read;
    }

    private static IEnumerable<WideRow> Generate(int count)
    {
        var rnd = new Random(12345);
        for (int i = 0; i < count; i++)
        {
            yield return new WideRow
            {
                IntCol = i,
                BigIntCol = (long)i * 1_000_000,
                BitCol = (i & 1) == 0,
                DecimalCol = Math.Round((decimal)(rnd.NextDouble() * 10_000), 4),
                FloatCol = rnd.NextDouble() * 1000,
                RealCol = (float)(rnd.NextDouble() * 100),
                GuidCol = Guid.NewGuid(),
                DateCol = new DateTime(2026, 1, 1).AddMinutes(i),
                OffsetCol = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(2)).AddMinutes(i),
                TextCol = (i % 7 == 0) ? null : $"row-{i}",
                StatusCol = (Status)(i % 3),
                QuantityCol = new Quantity(i % 500),
                NullableIntCol = (i % 5 == 0) ? null : i,
            };
        }
    }
}
