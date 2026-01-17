namespace FluentSqlLib;

public class Mapper
{
    public static T MapScalar<T>(object? scalar)
    {
        T result = default!;
        if (scalar is null || scalar == DBNull.Value)
        {
            if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) is null)
            {
                throw new InvalidOperationException($"Cannot convert null to non-nullable type {typeof(T).FullName}.");
            }

            return result;
        }

        return (T)Convert.ChangeType(scalar, typeof(T));
    }

    public static DbType GetDbType<T>()
    {
        throw new NotImplementedException();
    }
}
