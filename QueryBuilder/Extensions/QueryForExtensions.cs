using System;
using SqlKata.Compilers;

namespace SqlKata.Extensions
{
    public static class QueryForExtensions
    {
        public static Query ForFirebird(this Query src, Func<Query, Query> fn)
        {
            return src.For(EngineCodes.Firebird, fn);
        }

        public static Query ForMySql(this Query src, Func<Query, Query> fn)
        {
            return src.For(EngineCodes.MySql, fn);
        }

        public static Query ForOracle(this Query src, Func<Query, Query> fn)
        {
            return src.For(EngineCodes.Oracle, fn);
        }

        public static Query ForPostgreSql(this Query src, Func<Query, Query> fn)
        {
            return src.For(EngineCodes.PostgreSql, fn);
        }

        public static Query ForSqlite(this Query src, Func<Query, Query> fn)
        {
            return src.For(EngineCodes.Sqlite, fn);
        }

        public static Query ForSqlServer(this Query src, Func<Query, Query> fn)
        {
            return src.For(EngineCodes.SqlServer, fn);
        }

    }
}
