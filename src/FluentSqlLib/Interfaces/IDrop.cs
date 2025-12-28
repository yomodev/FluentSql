namespace FluentSqlLib.Interfaces;

public interface IDrop
{
    ValueTask<bool> DropAsync(CancellationToken cancellationToken = default);
}