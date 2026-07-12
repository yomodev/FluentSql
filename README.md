# FluentSql

A lightweight, fluent data-access library for SQL Server built on ADO.NET
(`Microsoft.Data.SqlClient`). You write the SQL; FluentSql handles
parameterization, high-performance result mapping, stored procedures, functions,
table-valued parameters, and streaming bulk insert — with no `DataTable`, no
per-row reflection, and both async and synchronous APIs.

It is **not** an ORM: there is no change tracking, LINQ-to-SQL translation, or
schema generation. It is a thin, fast, type-safe layer over raw SQL.

> Targets `net8.0`. SQL Server today; the provider seam (see
> [Extensibility](#extensibility)) is designed so other providers can be added
> later.

## Getting started

```csharp
using FluentSqlLib;
using Microsoft.Extensions.Logging.Abstractions;

var settings = new FluentSqlSettings
{
    ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=MyDb;Integrated Security=true;TrustServerCertificate=True;",
    // CommandTimeout = TimeSpan.FromSeconds(30) // optional
};

IFluentSql fluent = new FluentSql<FluentSqlSettings>(NullLogger<FluentSqlSettings>.Instance, settings);
```

`FluentSql<TSettings>` is a lightweight factory — creating one per unit of work is fine.

## Querying rows into a DTO

```csharp
public class User
{
    public int UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; }
}

var users = await fluent
    .Select("SELECT UserId, FirstName, LastName, CreatedAt FROM dbo.Users WHERE LastName = @last")
    .WithParam("@last", "Lovelace")
    .QueryAsync<User>()
    .ToListAsync();
```

Columns are matched to properties by name (case-insensitive). Mapping is done by
an expression-compiled delegate that is cached per `(type, result-set schema)`,
so there is no per-row reflection and no boxing of common types. Reads use
`CommandBehavior.SequentialAccess`; the mapper reads columns in ascending
ordinal order regardless of property declaration order.

### Mapping strategies

Each `QueryAsync<T>` / `Query<T>` (sync) has four forms:

```csharp
// 1. Convention + [SqlColumn] attributes (default)
.QueryAsync<User>()

// 2. Explicit property -> column map (full replacement of convention)
.QueryAsync<User>(new Dictionary<string,string> { ["GivenName"] = "FirstName" })

// 3. Column -> setter map (full replacement)
.QueryAsync<User>(new Dictionary<string, Action<User, IDataRecord>>
{
    ["FirstName"] = (u, r) => u.FirstName = r.GetString(r.GetOrdinal("FirstName")),
})

// 4. Fully custom mapper
.QueryAsync(r => new User { UserId = r.GetInt32(0), FirstName = r.GetString(1) })
```

`skipMissingColumns` (default `true`) leaves unmatched properties unset. Pass
`false` to throw an `InvalidOperationException` naming the missing column and
property.

### `[SqlColumn]` attribute

Control mapping without an explicit column map:

```csharp
public class Order
{
    [SqlColumn(Name = "OrderId", Identity = true)]
    public int Id { get; set; }

    [SqlColumn(Ordinal = 1)]
    public decimal Total { get; set; }

    [SqlColumn(Ignore = true)]
    public string? Scratch { get; set; }

    [SqlColumn(DbType = DbType.Decimal, Precision = 18, Scale = 4)]
    public decimal Amount { get; set; }
}
```

Properties: `Name`, `Ordinal`, `Ignore`, `DbType`, `Size`, `Precision`, `Scale`,
`Computed`, `Identity`. `Computed`/`Identity` properties are excluded from
generated INSERT parameters and from bulk insert (the database provides them),
but are **not** excluded from table-valued parameters (a TVP's shape is defined
by its table type — use `Ignore` to drop a property from a TVP).

## Single values

```csharp
var name = await fluent.Select("SELECT FirstName FROM dbo.Users WHERE UserId = @id")
    .WithParam("@id", 1)
    .GetAsync<string>("FirstName");         // null if no row / NULL

var total = await fluent.Select("SELECT Total FROM dbo.Orders WHERE OrderId = @id")
    .WithParam("@id", 7)
    .GetAsync<decimal>("Total", -1m);       // default if no row
```

## Stored procedures

```csharp
// Rows
var users = await fluent.StoredProcedure("dbo.sp_GetUsersByLastName")
    .WithParam("@last", "Hopper")
    .EnumerateAsync<User>()
    .ToListAsync();

// Output parameters (decimal output needs precision/scale, or a size for string)
var total = await fluent.StoredProcedure("dbo.sp_GetOrderTotal")
    .WithParam("@orderId", 42)
    .WithOutputParam<decimal>("@total", precision: 10, scale: 2)
    .GetOutputAsync<decimal>("total");

// Return value (Int32)
int rc = await fluent.StoredProcedure("dbo.sp_DoWork").GetAsync<int>();
```

### Multiple result sets

`QueryMultipleAsync` returns a forward-only reader; consume grids **in order**,
once each. Reading past the last grid throws.

```csharp
await using var results = await fluent.StoredProcedure("dbo.sp_UsersAndCount").QueryMultipleAsync();

var users = await results.ReadListAsync<User>();     // grid 1
int count  = await results.ReadScalarAsync<int>();   // grid 2

// Or stream the first grid:
await foreach (var u in results.ReadAsync<User>()) { /* ... */ }
```

## Functions

```csharp
var sum = await fluent.Function("dbo.fn_AddTwoInts")
    .WithParam("a", 3)
    .WithParam("b", 5)
    .GetAsync<int>();
```

## Bulk insert

`InsertManyAsync` streams rows straight into the table via `SqlBulkCopy` over a
forward-only reader — no `DataTable` is materialized, so the full source set is
never held in memory. Accessors are expression-compiled and cached per type.

```csharp
var rows = GetManyRows(); // IEnumerable<Product>, possibly lazy/streamed
long written = await fluent.Table("dbo.Products").InsertManyAsync(rows, batchSize: 5000);
```

`Identity`/`Computed` columns are excluded automatically.

## Table-valued parameters

```csharp
var orders = new[]
{
    new OrderRow { UserId = 1, Total = 10.5m },
    new OrderRow { UserId = 1, Total = 20.0m },
};

await fluent.StoredProcedure("dbo.sp_BulkInsertOrders")
    .WithParam("@orders", orders, "dbo.OrderTableType")
    .GetAsync<int>();
```

TVP columns bind to the user-defined table type **by position**; use
`[SqlColumn(Ordinal = n)]` when the DTO's declaration order differs.

## Enums and custom converters

Enums are stored as their numeric underlying type automatically (both directions,
including nullable enums) — no registration needed.

For custom property types, register a bidirectional converter once; it is used by
both the read mappers and the bulk-insert writer:

```csharp
ColumnConverterRegistry.Register<Quantity, int>(q => q.Value, v => new Quantity(v));
```

## Synchronous API

Every async member has a true synchronous counterpart (real
`Open`/`ExecuteReader`/`Read`, not `.GetAwaiter().GetResult()`): `Query<T>`,
`Get<T>`, `Enumerate<T>`, `GetOutput`, `InsertMany`, `QueryMultiple`
(returns an `IDisposable` reader with `ReadList<T>`/`ReadScalar<T>`), etc.

```csharp
var users = fluent.Select("SELECT * FROM dbo.Users").Query<User>().ToList();
```

## Database metadata & DDL

```csharp
var db = fluent.CurrentDatabase;
await foreach (var t in db.ListTablesAsync()) { /* schema.table */ }
await db.DropTableAsync("dbo.Temp");
await db.TruncateTableAsync("dbo.Staging");
```

DDL is scoped to the target database by pointing the connection at it, and the
SQL is generated through overridable `Build*Sql` / `QuoteIdentifier` methods.

## Error handling

A failed column-to-property conversion throws `FluentSqlMappingException`, which
names the column, its source DB type, the target property and type, and (when the
reader can still surface it) the offending value.

## Extensibility

`ClientBase<TSettings>` implements the shared behavior; `SqlServerClient<TSettings>`
is the SQL Server implementation. A future provider overrides `CreateConnection`,
the `Build*Sql`/`QuoteIdentifier` DDL methods, and parameter handling as needed —
`FluentDatabaseContext` and the mapping layer are provider-agnostic.

## Not yet implemented

- **Transactions** — `BeginTransaction`/`Commit`/`Rollback` exist but the
  commit/rollback wiring is a stub pending an API design decision.
- **Source generator** — a `[GenerateSqlMapper]`-driven compile-time mapper
  (to avoid runtime expression compilation and enable trimming/AOT) is planned.
- **Providers other than SQL Server.**
