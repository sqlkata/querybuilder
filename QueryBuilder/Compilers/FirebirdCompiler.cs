using System.Text.RegularExpressions;

namespace SqlKata.Compilers
{
    public class FirebirdCompiler : Compiler
    {
        public FirebirdCompiler()
        {
            EngineCode = EngineCodes.Firebird;
            SingleRowDummyTableName = "RDB$DATABASE";
            XService = new ("\"", "\"", "AS ", true);
        }

        public override SqlResult CompileInsertQuery(Query query)
        {
            var ctx = base.CompileInsertQuery(query);

            var inserts = ctx.Query.GetComponents<AbstractInsertClause>("insert", EngineCode);

            if (inserts.Count > 1)
            {
                ctx.ReplaceRaw(Regex.Replace(ctx.RawSql, @"\)\s+VALUES\s+\(", ") SELECT "));
                ctx.ReplaceRaw(Regex.Replace(ctx.RawSql, @"\),\s*\(", " FROM RDB$DATABASE UNION ALL SELECT "));
                ctx.ReplaceRaw(Regex.Replace(ctx.RawSql, @"\)$", " FROM RDB$DATABASE"));
            }

            return ctx;
        }

        public override string? CompileLimit(SqlResult ctx, Writer writer)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset > 0)
            {
                ctx.Bindings.Add(offset + 1);
                ctx.Bindings.Add(limit + offset);

                return $"ROWS {ParameterPlaceholder} TO {ParameterPlaceholder}";
            }

            return null;
        }


        protected override string CompileColumns(SqlResult ctx, Writer writer)
        {
            var compiled = base.CompileColumns(ctx, writer);

            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                ctx.Bindings.Insert(0, limit);

                ctx.Query.RemoveComponent("limit");

                return $"SELECT FIRST {ParameterPlaceholder}" + compiled.Substring(6);
            }

            if (limit == 0 && offset > 0)
            {
                ctx.Bindings.Insert(0, offset);

                ctx.Query.RemoveComponent("offset");

                return $"SELECT SKIP {ParameterPlaceholder}" + compiled.Substring(6);
            }

            return compiled;
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            var column = XService.Wrap(condition.Column);

            string left;

            if (condition.Part == "time")
                left = $"CAST({column} as TIME)";
            else if (condition.Part == "date")
                left = $"CAST({column} as DATE)";
            else
                left = $"EXTRACT({condition.Part.ToUpperInvariant()} FROM {column})";

            var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

            if (condition.IsNot) return $"NOT ({sql})";

            return sql;
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
