using FluentSqlLib.Interfaces;

namespace FluentSqlLib;

public interface IFluentTableContext : IDrop
{
    ValueTask<long> InsertManyAsync<T>(IEnumerable<T> rows, CancellationToken cancellationToken = default);
}