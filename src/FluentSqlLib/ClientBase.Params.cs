namespace FluentSqlLib;

/// <summary>Fluent parameter registration (input, output, and table-valued).</summary>
public abstract partial class ClientBase<TSettings>
{
    public virtual ISqlParam WithOutputParam<T>(string name)
    {
        parameters.Add(new QueryParameter<T>(name));
        return this;
    }

    public virtual ISqlParam WithOutputParam<T>(string name, byte precision, byte scale)
    {
        parameters.Add(new QueryParameter<T>(name) { Precision = precision, Scale = scale });
        return this;
    }

    public virtual ISqlParam WithParam<T>(string name, T value)
    {
        parameters.Add(new QueryParameter<T>(name, value));
        return this;
    }

    public virtual ISqlParam WithParam<T>(
        string name, IEnumerable<T> tableValued, string tableTypeName)
    {
        parameters.Add(new QueryParameter<T>
        (
            name,
            tableValued,
            tableTypeName
        ));

        return this;
    }
}
