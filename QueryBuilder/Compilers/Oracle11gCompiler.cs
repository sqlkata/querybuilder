using System;
using System.Linq;
using SqlKata.Compilers.Bindings;

// ReSharper disable InconsistentNaming

namespace SqlKata.Compilers
{
    public sealed class Oracle11gCompiler : Compiler
    {
        public Oracle11gCompiler() : base(
            new OracleResultBinder()
            )
        {
            ColumnAsKeyword = "";
            TableAsKeyword = "";
        }

        public override string EngineCode { get; } = Oracle11gCompilerExtensions.ENGINE_CODE;

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
        
        internal void ApplyLimit(SqlResult ctx)
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
                newSql = $"SELECT * FROM (SELECT {alias1}.*, ROWNUM {alias2} FROM ({ctx.RawSql}) {alias1}) WHERE {alias2} > ?";
                ctx.Bindings.Add(offset);
            } else if (offset == 0)
            {
                newSql = $"SELECT * FROM ({ctx.RawSql}) WHERE ROWNUM <= ?";
                ctx.Bindings.Add(limit);
            }
            else
            {
                newSql = $"SELECT * FROM (SELECT {alias1}.*, ROWNUM {alias2} FROM ({ctx.RawSql}) {alias1} WHERE ROWNUM <= ?) WHERE {alias2} > ?";
                ctx.Bindings.Add(limit +  offset);
                ctx.Bindings.Add(offset);
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
