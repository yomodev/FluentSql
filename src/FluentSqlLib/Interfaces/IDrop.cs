namespace FluentSqlLib.Interfaces;

public interface IDrop
{
    Task<bool> DropAsync(CancellationToken cancellationToken = default);
}