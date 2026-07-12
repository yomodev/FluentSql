using BenchmarkDotNet.Running;
using FluentSqlLib.Benchmarks;

// Requires SQL Server LocalDB (MSSQLLocalDB). Run in Release:
//   dotnet run -c Release --project bench/FluentSqlLib.Benchmarks
BenchmarkRunner.Run<RoundTripBenchmarks>();
