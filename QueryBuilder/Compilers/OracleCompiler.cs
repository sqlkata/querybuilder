using System.Diagnostics;
using System.Linq;
using SqlKata.Compilers.Extensions;

namespace SqlKata.Compilers
{
    public class OracleCompiler : Compiler
    {
        public OracleCompiler()
        {
            ColumnAsKeyword = "";
            TableAsKeyword = "";
            parameterPlaceholderPrefix = ":p";
        }

        public override string EngineCode { get; } = OracleCompilerExtensions.ENGINE_CODE;
        public bool UseLegacyPagination { get; set; } = false;

        protected override SqlResult CompileSelectQuery(Query query)
        {
            if (!UseLegacyPagination)
            {
                return base.CompileSelectQuery(query);
            }

            var result = base.CompileSelectQuery(query);

            ApplyLegacyLimit(result);

            return result;
        }

        public override string CompileLimit(SqlResult ctx)
        {
            if (UseLegacyPagination)
            {
                // in pre-12c versions of Oracle, limit is handled by ROWNUM techniques
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

        internal void ApplyLegacyLimit(SqlResult ctx)
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
            }
            else if (offset == 0)
            {
                newSql = $"SELECT * FROM ({ctx.RawSql}) WHERE ROWNUM <= ?";
                ctx.Bindings.Add(limit);
            }
            else
            {
                newSql = $"SELECT * FROM (SELECT {alias1}.*, ROWNUM {alias2} FROM ({ctx.RawSql}) {alias1} WHERE ROWNUM <= ?) WHERE {alias2} > ?";
                ctx.Bindings.Add(limit + offset);
                ctx.Bindings.Add(offset);
            }

            ctx.RawSql = newSql;
        }
    }
}
