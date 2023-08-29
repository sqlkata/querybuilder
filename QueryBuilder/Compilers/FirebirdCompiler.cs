using System.Text.RegularExpressions;

namespace SqlKata.Compilers
{
    public class FirebirdCompiler : Compiler
    {
        public FirebirdCompiler()
        {
            EngineCode = EngineCodes.Firebird;
            SingleRowDummyTableName = "RDB$DATABASE";
            XService = new("\"", "\"", "AS ", true);
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

        protected override string? CompileLimit(SqlResult ctx, Writer writer)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset > 0)
            {
                ctx.Bindings.Add(offset + 1);
                ctx.Bindings.Add(limit + offset);

                writer.S.Append("ROWS ? TO ?");
                return writer;
            }

            return null;
        }


        protected override string CompileColumns(SqlResult ctx, Writer writer)
        {
            var compiled = base.CompileColumns(ctx, writer.Sub());

            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                ctx.Bindings.Insert(0, limit);

                ctx.Query.RemoveComponent("limit");

                writer.S.Append("SELECT FIRST ?");
                writer.S.Append(compiled.Substring(6));
                return writer;
            }

            if (limit == 0 && offset > 0)
            {
                ctx.Bindings.Insert(0, offset);

                ctx.Query.RemoveComponent("offset");

                writer.S.Append("SELECT SKIP ?");
                writer.S.Append(compiled.Substring(6));
                return writer;
            }

            writer.S.Append(compiled);
            return writer;
        }

        protected override void CompileBasicDateCondition(SqlResult ctx, BasicDateCondition x, Writer writer)
        {
            if (x.IsNot)
                writer.S.Append("NOT (");
            if (x.Part == "time")
            {
                writer.S.Append("CAST(");
                writer.AppendName(x.Column);
                writer.S.Append(" as TIME) ");
            }
            else if (x.Part == "date")
            {
                writer.S.Append("CAST(");
                writer.AppendName(x.Column);
                writer.S.Append(" as DATE) ");
            }
            else
            {
                writer.S.Append("EXTRACT(");
                writer.AppendName(x.Part.ToUpperInvariant());
                writer.S.Append(" FROM ");
                writer.AppendName(x.Column);
                writer.S.Append(") ");
            }
            writer.S.Append(Operators.CheckOperator(x.Operator));
            writer.S.Append(" ");
            writer.S.Append(Parameter(ctx, x.Value));
            if (x.IsNot)
                writer.S.Append(")");
        }


        protected override string CompileTrue()
        {
            return "1";
        }

        protected override string CompileFalse()
        {
            return "0";
        }
    }
}
