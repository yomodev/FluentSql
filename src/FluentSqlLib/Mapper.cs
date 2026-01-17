using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

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

    public sealed class DbTypeInfo
    {
        public DbType DbType { get; }
        public byte? Precision { get; }
        public byte? Scale { get; }
        public int? Size { get; }
        public bool IsJson { get; }

        public DbTypeInfo(DbType dbType, byte? precision = null, byte? scale = null, int? size = null, bool isJson = false)
        {
            DbType = dbType;
            Precision = precision;
            Scale = scale;
            Size = size;
            IsJson = isJson;
        }
    }

    public static class DbTypeMapper
    {
        private static readonly Dictionary<Type, DbType> ClrToDb = new()
        {
            { typeof(byte), DbType.Byte },
            { typeof(short), DbType.Int16 },
            { typeof(int), DbType.Int32 },
            { typeof(long), DbType.Int64 },
            { typeof(bool), DbType.Boolean },
            { typeof(string), DbType.String },
            { typeof(decimal), DbType.Decimal },
            { typeof(double), DbType.Double },
            { typeof(float), DbType.Single },
            { typeof(DateTime), DbType.DateTime2 },
            { typeof(DateTimeOffset), DbType.DateTimeOffset },
            { typeof(Guid), DbType.Guid },
            { typeof(byte[]), DbType.Binary },
            { typeof(TimeSpan), DbType.Time }
        };

        public static DbTypeInfo FromClr(Type type, byte? precision = null, byte? scale = null, int? size = null)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);

            if (type == typeof(JsonDocument) || type == typeof(object))
                return new DbTypeInfo(DbType.String, size: -1, isJson: true);

            if (!ClrToDb.TryGetValue(type, out var dbType))
                throw new NotSupportedException($"Unsupported CLR type {type}");

            return new DbTypeInfo(dbType, precision, scale, size);
        }
    }

    public static class SqlDbTypeMapper
    {
        private static readonly Dictionary<DbType, SqlDbType> DbToSql = new()
        {
            { DbType.Byte, SqlDbType.TinyInt },
            { DbType.Int16, SqlDbType.SmallInt },
            { DbType.Int32, SqlDbType.Int },
            { DbType.Int64, SqlDbType.BigInt },
            { DbType.Boolean, SqlDbType.Bit },
            { DbType.String, SqlDbType.NVarChar },
            { DbType.Decimal, SqlDbType.Decimal },
            { DbType.Double, SqlDbType.Float },
            { DbType.Single, SqlDbType.Real },
            { DbType.DateTime2, SqlDbType.DateTime2 },
            { DbType.DateTimeOffset, SqlDbType.DateTimeOffset },
            { DbType.Guid, SqlDbType.UniqueIdentifier },
            { DbType.Binary, SqlDbType.VarBinary },
            { DbType.Time, SqlDbType.Time }
        };

        public static SqlDbType ToSql(DbTypeInfo info)
        {
            if (!DbToSql.TryGetValue(info.DbType, out var sql))
                throw new NotSupportedException($"No SqlDbType for {info.DbType}");

            return sql;
        }
    }

    public static class SqlParameterFactory
    {
        public static IEnumerable<SqlParameter> CreateParameters(object poco)
        {
            foreach (var prop in poco.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(poco);
                var typeInfo = DbTypeMapper.FromClr(prop.PropertyType);

                var p = new SqlParameter("@" + prop.Name, SqlDbTypeMapper.ToSql(typeInfo))
                {
                    Value = value ?? DBNull.Value
                };

                if (typeInfo.Precision.HasValue) p.Precision = typeInfo.Precision.Value;
                if (typeInfo.Scale.HasValue) p.Scale = typeInfo.Scale.Value;
                if (typeInfo.Size.HasValue) p.Size = typeInfo.Size.Value;

                yield return p;
            }
        }
    }

    public static class DataReaderMapper
    {
        public static List<T> MapToList<T>(SqlDataReader reader) where T : new()
        {
            var result = new List<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            while (reader.Read())
            {
                var item = new T();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var colName = reader.GetName(i);

                    if (!props.TryGetValue(colName, out var prop))
                        continue;

                    var val = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    if (val != null && prop.PropertyType == typeof(JsonDocument))
                        val = JsonDocument.Parse(val.ToString());

                    prop.SetValue(item, val);
                }

                result.Add(item);
            }

            return result;
        }
    }

    public static class TvpBuilder
    {
        public static SqlParameter CreateTvp<T>(
            string name,
            IEnumerable<T> rows,
            string typeName)
        {
            var table = new DataTable();
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                table.Columns.Add(prop.Name, type);
            }

            foreach (var row in rows)
            {
                var values = props.Select(p => p.GetValue(row) ?? DBNull.Value).ToArray();
                table.Rows.Add(values);
            }

            return new SqlParameter(name, SqlDbType.Structured)
            {
                TypeName = typeName,
                Value = table
            };
        }
    }
}


public static class RuntimeMapper
{
    private static readonly ConcurrentDictionary<string, Delegate> _cache = new();

    public static Func<SqlDataReader, T> GetMapper<T>(SqlDataReader reader) where T : new()
    {
        var key = $"{typeof(T).FullName}:{SchemaHash(reader)}";
        return (Func<SqlDataReader, T>)_cache.GetOrAdd(key, _ => BuildMapper<T>(reader));
    }

    private static Func<SqlDataReader, T> BuildMapper<T>(SqlDataReader reader) where T : new()
    {
        var r = Expression.Parameter(typeof(SqlDataReader), "r");
        var obj = Expression.Variable(typeof(T), "o");
        var body = new List<Expression>
        {
            Expression.Assign(obj, Expression.New(typeof(T)))
        };

        foreach (var p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            int ord;
            try { ord = reader.GetOrdinal(p.Name); }
            catch { continue; }

            var isDbNull = Expression.Call(r, nameof(SqlDataReader.IsDBNull), null, Expression.Constant(ord));
            var getVal = Expression.Call(r, nameof(SqlDataReader.GetFieldValue), new[] { p.PropertyType }, Expression.Constant(ord));

            var assign = Expression.Assign(Expression.Property(obj, p), getVal);
            body.Add(Expression.IfThen(Expression.Not(isDbNull), assign));
        }

        body.Add(obj);
        var block = Expression.Block(new[] { obj }, body);
        return Expression.Lambda<Func<SqlDataReader, T>>(block, r).Compile();
    }

    private static string SchemaHash(SqlDataReader r)
    {
        Span<byte> buffer = stackalloc byte[256];
        int idx = 0;
        for (int i = 0; i < r.FieldCount && idx < buffer.Length; i++)
        {
            var name = r.GetName(i);
            foreach (var c in name)
                buffer[idx++] = (byte)c;
        }
        return idx.ToString();
    }

    public static async IAsyncEnumerable<T> StreamAsync<T>(
        SqlCommand cmd,
        [EnumeratorCancellation] CancellationToken ct = default) where T : new()
    {
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var mapper = GetMapper<T>(reader);

        while (await reader.ReadAsync(ct))
            yield return mapper(reader);
    }
}
