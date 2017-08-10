using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata.Compilers
{
    public class SqlServerCompiler : Compiler
    {
        public SqlServerCompiler()
        {
            EngineCode = "sqlsrv";
        }

        protected override string OpeningIdentifier()
        {
            return "[";
        }

        protected override string ClosingIdentifier()
        {
            return "]";
        }

        protected override Query OnBeforeSelect(Query query)
        {
            var limitOffset = query.GetOne<LimitOffset>("limit", EngineCode);

            if (limitOffset == null || !limitOffset.HasOffset())
            {
                return query;
            }


            // Surround the original query with a parent query, then restrict the result to the offset provided, see more at https://docs.microsoft.com/en-us/sql/t-sql/functions/row-number-transact-sql


            var rowNumberColName = "row_num";

            var orderStatement = CompileOrders(query) ?? "ORDER BY (SELECT 0)";

            var orderClause = query.Get("order", EngineCode);


            // get a clone without the limit and order
            query.Clear("order");
            query.Clear("limit");
            var subquery = query.Clone();

            subquery.Clear("cte");

            // Now clear other stuff
            query.Clear("select");
            query.Clear("from");
            query.Clear("join");
            query.Clear("where");
            query.Clear("group");
            query.Clear("having");
            query.Clear("union");

            // Transform the query to make it a parent query
            query.Select("*");

            if (!subquery.Has("columns", EngineCode))
            {
                subquery.SelectRaw("*");
            }

            // Add the row_number select, and put back the bindings here if any
            subquery.SelectRaw(
                    $"ROW_NUMBER() OVER ({orderStatement}) AS {WrapValue(rowNumberColName)}",
                    orderClause.SelectMany(x => x.GetBindings(EngineCode))
            );

            query.From(subquery);

            if (limitOffset.HasLimit())
            {
                query.WhereBetween(
                    rowNumberColName,
                    limitOffset.Offset + 1,
                    limitOffset.Limit + limitOffset.Offset
                );
            }
            else
            {
                query.Where(rowNumberColName, ">=", limitOffset.Offset + 1);
            }

            limitOffset.Clear();

            return query;

        }

        protected override string CompileColumns(Query query)
        {
            var compiled = base.CompileColumns(query);

            // If there is a limit on the query, but not an offset, we will add the top
            // clause to the query, which serves as a "limit" type clause within the
            // SQL Server system similar to the limit keywords available in MySQL.
            var limitOffset = query.GetOne("limit", EngineCode) as LimitOffset;

            if (limitOffset != null && limitOffset.HasLimit() && !limitOffset.HasOffset())
            {
                // Add a fake raw select to simulate the top bindings 
                query.Clauses.Insert(0, new RawColumn
                {
                    Engine = EngineCode,
                    Component = "select",
                    Expression = "",
                    Bindings = new object[] { limitOffset.Limit }
                });

                query.Clear("limit");

                return "SELECT TOP ?" + compiled.Substring(6);
            }

            return compiled;
        }

        public override string CompileLimit(Query query)
        {
            return "";
        }

        public override string CompileOffset(Query query)
        {

            return "";
        }

        public override string CompileRandom(string seed)
        {
            return "NEWID()";
        }
    }

    public static class SqlServerCompilerExtensions
    {
        public static string ENGINE_CODE = "sqlsrv";
        public static Query ForSqlServer(this Query src, Func<Query, Query> fn)
        {
            return src.For(SqlServerCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}