using System;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace SqlKata.Compilers
{
    public sealed class Oracle11gCompiler : Compiler
    {
        public Oracle11gCompiler()
        {
            EngineCode = Oracle11gCompilerExtensions.ENGINE_CODE;
            ColumnAsKeyword = "";
            TableAsKeyword = "";
        }
        
        protected override SqlResult CompileSelectQuery(Query query)
        {
            query = PrepareLimit(query);
            var ctx = new SqlResult
            {
                Query = query.Clone(),
            };
            
            var results = new[] {
                    CompileColumns(ctx),
                    CompileFrom(ctx),
                    CompileJoins(ctx),
                    CompileWheres(ctx),
                    CompileGroups(ctx),
                    CompileHaving(ctx),
                    CompileOrders(ctx),
                    CompileUnion(ctx)
                }
                .Where(x => x != null)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            var sql = string.Join(" ", results);

            ctx.RawSql = sql;

            return ctx;
        }

        public override string CompileLimit(SqlResult ctx)
        {
            throw new NotSupportedException();
        }

        public Query PrepareLimit(Query query)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return query;
            }

            var innerQuery = query.Clone();
            innerQuery.ClearComponent("limit");
            var newQuery = new Query().From(innerQuery); 
            
            if (limit == 0)
            {
                newQuery
                    .From(q => q.Select("a.*")
                        .SelectRaw("ROWNUM r__")
                        .From(innerQuery.As("a"))
                        .WhereRaw($"ROWNUM < {offset+limit}"))
                    .WhereRaw($"r__ > {offset}");
                return newQuery;
            }
            
            if (offset == 0)
            {
                newQuery.From(innerQuery).WhereRaw($"ROWNUM <= {limit}");
                return newQuery;
            }

            newQuery
                .From(q => q.Select("a.*")
                            .SelectRaw("ROWNUM r__")
                            .From(innerQuery.As("a"))
                            .WhereRaw($"ROWNUM <= {offset+limit}"))
                .WhereRaw($"r__ > {offset}");
            return newQuery;
        }
    }

    public static class Oracle11gCompilerExtensions
    {
        public static string ENGINE_CODE = "oracle11g";

        public static Query ForOracle11g(this Query src, Func<Query, Query> fn)
        {
            return src.For(ENGINE_CODE, fn);
        }
    }
}
