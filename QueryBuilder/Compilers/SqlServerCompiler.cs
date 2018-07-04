using System;
using System.Linq;

namespace SqlKata.Compilers
{
    public class SqlServerCompiler : Compiler
    {
        public SqlServerCompiler()
        {
            EngineCode = "sqlsrv";
            OpeningIdentifier = "[";
            ClosingIdentifier = "]";
        }

        protected override SqlResult CompileSelectQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query,
            };

            var limitOffset = ctx.Query.GetOneComponent<LimitOffset>("limit", EngineCode);

            var hasOffset = limitOffset?.HasOffset() ?? false;
            var hasLimit = limitOffset?.HasLimit() ?? false;

            if (!ctx.Query.HasComponent("select", EngineCode))
            {
                ctx.Query.Select("*");
            }

            if (hasOffset)
            {
                var orderStatement = CompileOrders(ctx) ?? "ORDER BY (SELECT 0)";
                ctx.Query.SelectRaw($"ROW_NUMBER() OVER ({orderStatement}) AS [row_num]");
            }


            var results = new[] {
                    this.CompileColumns(ctx),
                    this.CompileFrom(ctx),
                    this.CompileJoins(ctx),
                    this.CompileWheres(ctx),
                    this.CompileGroups(ctx),
                    this.CompileHaving(ctx),
                    hasOffset ? null : this.CompileOrders(ctx),
                    this.CompileUnion(ctx),
                }
               .Where(x => x != null)
               .Select(x => x.Trim())
               .Where(x => !string.IsNullOrEmpty(x))
               .ToList();

            string sql = string.Join(" ", results);

            if (hasOffset)
            {
                if (hasLimit)
                {
                    sql = $"SELECT * FROM ({sql}) AS [results_wrapper] WHERE [row_num] BETWEEN ? AND ?";
                    ctx.Bindings.Add(limitOffset.Offset + 1);
                    ctx.Bindings.Add(limitOffset.Limit + limitOffset.Offset);
                }
                else
                {
                    sql = $"SELECT * FROM ({sql}) AS [results_wrapper] WHERE [row_num] >= ?";
                    ctx.Bindings.Add(limitOffset.Offset + 1);
                }
            }

            ctx.RawSql = sql;

            return ctx;
        }

        protected override SqlResult OnBeforeSelect(SqlResult ctx)
        {
            var limitOffset = ctx.Query.GetOneComponent<LimitOffset>("limit", EngineCode);

            if (limitOffset == null || !limitOffset.HasOffset())
            {
                return ctx;
            }


            // Surround the original query with a parent query, then restrict the result to the offset provided, see more at https://docs.microsoft.com/en-us/sql/t-sql/functions/row-number-transact-sql


            var rowNumberColName = "row_num";

            var orderStatement = CompileOrders(ctx) ?? "ORDER BY (SELECT 0)";

            var orderClause = ctx.Query.GetComponents("order", EngineCode);


            // get a clone without the limit and order
            ctx.Query.ClearComponent("order");
            ctx.Query.ClearComponent("limit");
            var subquery = ctx.Query.Clone();

            subquery.ClearComponent("cte");

            // Now clear other stuff
            ctx.Query.ClearComponent("select");
            ctx.Query.ClearComponent("from");
            ctx.Query.ClearComponent("join");
            ctx.Query.ClearComponent("where");
            ctx.Query.ClearComponent("group");
            ctx.Query.ClearComponent("having");
            ctx.Query.ClearComponent("union");

            // Transform the query to make it a parent query
            ctx.Query.Select("*");

            if (!subquery.HasComponent("select", EngineCode))
            {
                subquery.Select("*");
            }

            //Add an alias name to the subquery
            subquery.As("subquery");

            // Add the row_number select, and put back the bindings here if any
            subquery.SelectRaw(
                    $"ROW_NUMBER() OVER ({orderStatement}) AS {WrapValue(rowNumberColName)}",
                    new object[] { }
            );

            ctx.Query.From(subquery);

            if (limitOffset.HasLimit())
            {
                ctx.Query.WhereBetween(
                    rowNumberColName,
                    limitOffset.Offset + 1,
                    limitOffset.Limit + limitOffset.Offset
                );
            }
            else
            {
                ctx.Query.Where(rowNumberColName, ">=", limitOffset.Offset + 1);
            }

            limitOffset.Clear();

            return ctx;

        }

        protected override string CompileColumns(SqlResult ctx)
        {
            var compiled = base.CompileColumns(ctx);

            // If there is a limit on the query, but not an offset, we will add the top
            // clause to the query, which serves as a "limit" type clause within the
            // SQL Server system similar to the limit keywords available in MySQL.
            var limitOffset = ctx.Query.GetOneComponent("limit", EngineCode) as LimitOffset;

            if (limitOffset != null && limitOffset.HasLimit() && !limitOffset.HasOffset())
            {
                // top bindings should be inserted first
                ctx.Bindings.Insert(0, limitOffset.Limit);

                ctx.Query.ClearComponent("limit");

                return "SELECT TOP (?)" + compiled.Substring(6);
            }

            return compiled;
        }

        public override string CompileLimit(SqlResult ctx)
        {
            return "";
        }

        public override string CompileOffset(SqlResult ctx)
        {

            return "";
        }

        public override string CompileRandom(string seed)
        {
            return "NEWID()";
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);

            string left;

            if (condition.Part == "time")
            {
                left = $"CAST({column} as time)";
            }
            else if (condition.Part == "date")
            {
                left = $"CAST({column} as date)";
            }
            else
            {
                left = $"DATEPART({condition.Part.ToUpper()}, {column})";
            }

            var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
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
