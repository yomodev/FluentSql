using FluentSqlLib.Interfaces;

namespace FluentSqlLib;

public interface IFluentTableContext : IDrop
{
    Task<long> InsertManyAsync<T>(IEnumerable<T> rows, CancellationToken cancellationToken = default);
}