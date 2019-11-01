using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlKata.Compilers
{
    public class FirebirdCompiler : Compiler
    {
        public FirebirdCompiler()
        {
        }

        public override string EngineCode { get; } = EngineCodes.Firebird;

        protected override SqlResult CompileInsertQuery(Query query)
        {
            SqlResult ctx = base.CompileInsertQuery(query);

            List<AbstractInsertClause> inserts = ctx.Query.GetComponents<AbstractInsertClause>("insert", EngineCode);

            if (inserts.Count > 1)
            {
                ctx.RawSql = Regex.Replace(ctx.RawSql, @"\)\s+VALUES\s+\(", ") SELECT ");
                ctx.RawSql = Regex.Replace(ctx.RawSql, @"\),\s*\(", " FROM RDB$DATABASE UNION ALL SELECT ");
                ctx.RawSql = Regex.Replace(ctx.RawSql, @"\)$", " FROM RDB$DATABASE");
            }

            return ctx;
        }

        public override string CompileLimit(SqlResult ctx)
        {
            int limit = ctx.Query.GetLimit(EngineCode);
            int offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset > 0)
            {
                ctx.Bindings.Add(offset + 1);
                ctx.Bindings.Add(limit + offset);

                return "ROWS ? TO ?";
            }

            return null;
        }


        protected override string CompileColumns(SqlResult ctx)
        {
            string compiled = base.CompileColumns(ctx);

            int limit = ctx.Query.GetLimit(EngineCode);
            int offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                ctx.Bindings.Insert(0, limit);

                ctx.Query.ClearComponent("limit");

                return "SELECT FIRST ?" + compiled.Substring(6);
            }
            else if (limit == 0 && offset > 0)
            {
                ctx.Bindings.Insert(0, offset);

                ctx.Query.ClearComponent("offset");

                return "SELECT SKIP ?" + compiled.Substring(6);
            }

            return compiled;
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            string column = Wrap(condition.Column);

            string left;

            if (condition.Part == "time")
            {
                left = $"CAST({column} as TIME)";
            }
            else if (condition.Part == "date")
            {
                left = $"CAST({column} as DATE)";
            }
            else
            {
                left = $"EXTRACT({condition.Part.ToUpperInvariant()} FROM {column})";
            }

            string sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        public override string WrapValue(string value)
        {
            return base.WrapValue(value).ToUpperInvariant();
        }

        public override string CompileTrue()
        {
            return "1";
        }

        public override string CompileFalse()
        {
            return "0";
        }
    }
}
