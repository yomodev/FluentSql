using System.Diagnostics.CodeAnalysis;

namespace FluentSqlLib;

public class QueryParameter<T> : QueryParameter
{
    [SetsRequiredMembers]
    public QueryParameter(string name, T? value)
    {
        Name = name;
        Value = value;
    }

    [SetsRequiredMembers]
    public QueryParameter(
        string name, IEnumerable<T> tableValued, string tableTypeName)
    {
        Name = name;
        Value = tableValued;
        TableTypeName = tableTypeName;
    }

    public IEnumerable<T> TableValued => Value as IEnumerable<T> 
        ?? throw new InvalidOperationException(
            $"Parameter '{Name}' is not set to a table-valued parameter of type '{typeof(T).FullName}'.");

    public string TableTypeName { get; } = string.Empty;
}

public class QueryParameter
{
    public required string Name { get; set; }
    
    public object? Value { get; set; }
}