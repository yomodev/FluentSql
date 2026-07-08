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
        return DbTypeMapper.FromClr(typeof(T)).DbType;
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
            foreach (var column in ColumnMap.ResolveAll(poco.GetType()))
            {
                if (column.Computed || column.Identity)
                    continue;

                var value = column.Property.GetValue(poco);
                var typeInfo = column.DbType.HasValue
                    ? new DbTypeInfo(column.DbType.Value, column.Precision, column.Scale, column.Size)
                    : DbTypeMapper.FromClr(column.Property.PropertyType, column.Precision, column.Scale, column.Size);

                var p = new SqlParameter("@" + column.ColumnName, SqlDbTypeMapper.ToSql(typeInfo))
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
            var columns = ColumnMap.ResolveAll(typeof(T));
            var byOrdinal = columns.Where(c => c.Ordinal.HasValue).ToDictionary(c => c.Ordinal!.Value);
            var byName = columns.Where(c => !c.Ordinal.HasValue)
                                 .ToDictionary(c => c.ColumnName, StringComparer.OrdinalIgnoreCase);

            while (reader.Read())
            {
                var item = new T();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (!byOrdinal.TryGetValue(i, out var column)
                        && !byName.TryGetValue(reader.GetName(i), out column))
                    {
                        continue;
                    }

                    var val = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    if (val != null && column.Property.PropertyType == typeof(JsonDocument))
                        val = JsonDocument.Parse(val.ToString());

                    column.Property.SetValue(item, val);
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
            => CreateTvp(name, rows, typeof(T), typeName);

        public static SqlParameter CreateTvp(
            string name,
            System.Collections.IEnumerable rows,
            Type rowType,
            string typeName)
        {
            var table = BuildDataTable(rows, rowType);

            return new SqlParameter(name, SqlDbType.Structured)
            {
                TypeName = typeName,
                Value = table
            };
        }

        public static DataTable BuildDataTable(System.Collections.IEnumerable rows, Type rowType)
        {
            var table = new DataTable();
            // Table-valued parameters bind columns by ordinal position against the SQL Server
            // user-defined table type, so an explicit Ordinal must control column order here.
            var columns = ColumnMap.ResolveAll(rowType).OrderBy(c => c.Ordinal ?? int.MaxValue).ToArray();

            foreach (var column in columns)
            {
                var type = Nullable.GetUnderlyingType(column.Property.PropertyType) ?? column.Property.PropertyType;
                table.Columns.Add(column.ColumnName, type);
            }

            foreach (var row in rows)
            {
                var values = columns.Select(c => c.Property.GetValue(row) ?? DBNull.Value).ToArray();
                table.Rows.Add(values);
            }

            return table;
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

        foreach (var column in ColumnMap.ResolveAll(typeof(T)))
        {
            int ord;
            if (column.Ordinal.HasValue)
            {
                ord = column.Ordinal.Value;
            }
            else
            {
                try { ord = reader.GetOrdinal(column.ColumnName); }
                catch { continue; }
            }

            var isDbNull = Expression.Call(r, nameof(SqlDataReader.IsDBNull), null, Expression.Constant(ord));
            var getVal = Expression.Call(r, nameof(SqlDataReader.GetFieldValue), new[] { column.Property.PropertyType }, Expression.Constant(ord));

            var assign = Expression.Assign(Expression.Property(obj, column.Property), getVal);
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
