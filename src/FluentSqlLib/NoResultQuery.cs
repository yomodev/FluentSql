using FluentSqlLib.Interfaces;

namespace FluentSqlLib;

internal class NoResultQuery(string sql) : Query(sql), INoResultQuery
{
}