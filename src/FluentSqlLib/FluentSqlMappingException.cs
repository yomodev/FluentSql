namespace FluentSqlLib;

/// <summary>
/// Thrown when a result-set column cannot be mapped onto a DTO property (type mismatch,
/// failed conversion, etc.). The message names the offending column, its value, and the
/// target property/type so the failure is diagnosable without a debugger.
/// </summary>
public sealed class FluentSqlMappingException : Exception
{
    public string ColumnName { get; }
    public string PropertyName { get; }
    public Type TargetType { get; }

    public FluentSqlMappingException(string columnName, string propertyName, Type targetType, string message, Exception inner)
        : base(message, inner)
    {
        ColumnName = columnName;
        PropertyName = propertyName;
        TargetType = targetType;
    }
}
