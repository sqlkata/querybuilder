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

            ApplyLimit(ctx);

            return ctx;
        }

        public override string CompileLimit(SqlResult ctx)
        {
            throw new NotSupportedException();
        }
        
        private void ApplyLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return;
            }
            
            //@todo replace with alias generator
            var alias1 = WrapValue("SqlKata_A__");
            var alias2 = WrapValue("SqlKata_B__"); 

            string newSql;
            if (limit == 0)
            {
                newSql = $"SELECT * FROM (SELECT {alias1}.*, ROWNUM {alias2} FROM ({ctx.RawSql}) {alias1}) WHERE {alias2} > {offset}";
            } else if (offset == 0)
            {
                newSql = $"SELECT * FROM ({ctx.RawSql}) WHERE ROWNUM <= {limit}";
            }
            else
            {
                newSql = $"SELECT * FROM (SELECT {alias1}.*, ROWNUM {alias2} FROM ({ctx.RawSql}) {alias1} WHERE ROWNUM <= {limit + offset}) WHERE {alias2} > {offset}";
            }

            ctx.RawSql = newSql;
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
