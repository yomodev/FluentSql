using FluentSqlLib.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace FluentSqlLib;

public class QueryParameter<T> : QueryParameter, IGenericQueryParameter
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

    public Type Type => typeof(T);
}

// TODO set DbType appropriately

public class QueryParameter
{
    public DbType DbType { get; set; }
    
    public bool IsOutput { get; set; } = false;
    
    public required string Name { get; set; }
    
    public byte? Precision { get; set; }
    
    public int? Size { get; set; }
    
    public byte? Scale { get; set; }
    
    public string TableTypeName { get; set; } = string.Empty;
    
    public object? Value { get; set; }
}