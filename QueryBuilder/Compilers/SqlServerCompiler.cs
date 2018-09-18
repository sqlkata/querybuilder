using System;
using System.Linq;
using SqlKata.Interfaces;

namespace SqlKata.Compilers
{
    public class SqlServerCompiler : Compiler
    {
        public bool UseLegacyPagination { get; set; } = true;

        public SqlServerCompiler()
        {
            EngineCode = "sqlsrv";
            OpeningIdentifier = "[";
            ClosingIdentifier = "]";
        }

        protected override SqlResult CompileSelectQuery(IQuery query)
        {
            if (!UseLegacyPagination || !query.HasOffset())
            {
                return base.CompileSelectQuery(query);
            }

            query = query.Clone();

            var ctx = new SqlResult
            {
                Query = query,
            };

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);


            if (!query.HasComponent("select"))
            {
                query.Select("*");
            }
            var order = CompileOrders(ctx) ?? "ORDER BY (SELECT 0)";
            query.SelectRaw($"ROW_NUMBER() OVER ({order}) AS [row_num]", ctx.Bindings);

            query.ClearComponent("order");


            var result = base.CompileSelectQuery(query);

            if (limit == 0)
            {
                result.RawSql = $"SELECT * FROM ({result.RawSql}) AS [results_wrapper] WHERE [row_num] >= ?";
                result.Bindings.Add(offset + 1);
            }
            else
            {
                result.RawSql = $"SELECT * FROM ({result.RawSql}) AS [results_wrapper] WHERE [row_num] BETWEEN ? AND ?";
                result.Bindings.Add(offset + 1);
                result.Bindings.Add(limit + offset);
            }

            return result;
        }

        protected override string CompileColumns(SqlResult ctx)
        {
            var compiled = base.CompileColumns(ctx);

            if (!UseLegacyPagination)
            {
                return compiled;
            }

            // If there is a limit on the query, but not an offset, we will add the top
            // clause to the query, which serves as a "limit" type clause within the
            // SQL Server system similar to the limit keywords available in MySQL.
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                // top bindings should be inserted first
                ctx.Bindings.Insert(0, limit);

                ctx.Query.ClearComponent("limit");

                // handle distinct
                if (compiled.IndexOf("SELECT DISTINCT") == 0)
                {
                    return "SELECT DISTINCT TOP (?)" + compiled.Substring(15);
                }

                return "SELECT TOP (?)" + compiled.Substring(6);
            }

            return compiled;
        }

        public override string CompileLimit(SqlResult ctx)
        {
            if (UseLegacyPagination)
            {
                // in legacy versions of Sql Server, limit is handled by TOP
                // and ROW_NUMBER techniques
                return null;
            }

            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return null;
            }

            var safeOrder = "";
            if (!ctx.Query.HasComponent("order"))
            {
                safeOrder = "ORDER BY (SELECT 0) ";
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                return $"{safeOrder}OFFSET ? ROWS";
            }

            ctx.Bindings.Add(offset);
            ctx.Bindings.Add(limit);

            return $"{safeOrder}OFFSET ? ROWS FETCH NEXT ? ROWS ONLY";
        }

        public override string CompileRandom(string seed)
        {
            return "NEWID()";
        }

        public override string CompileTrue()
        {
            return "cast(1 as bit)";
        }

        public override string CompileFalse()
        {
            return "cast(0 as bit)";
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
        public static IQuery ForSqlServer(this IQuery src, Func<IQuery, IQuery> fn)
        {
            return src.For(SqlServerCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}
