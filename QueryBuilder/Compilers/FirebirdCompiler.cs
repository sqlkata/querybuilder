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

        public override void CompileInsertQueryInner(SqlResult ctx, Query query, Writer writer)
        {
            base.CompileInsertQueryInner(ctx, query, writer);

            var inserts = query.GetComponents<AbstractInsertClause>("insert", EngineCode);

            if (inserts.Count > 1)
            {
                ctx.ReplaceRaw(Regex.Replace(ctx.RawSql, @"\)\s+VALUES\s+\(", ") SELECT "));
                ctx.ReplaceRaw(Regex.Replace(ctx.RawSql, @"\),\s*\(", " FROM RDB$DATABASE UNION ALL SELECT "));
                ctx.ReplaceRaw(Regex.Replace(ctx.RawSql, @"\)$", " FROM RDB$DATABASE"));
            }
        }

        protected override string? CompileLimit(SqlResult ctx, Query query, Writer writer)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit > 0 && offset > 0)
            {
                ctx.BindingsAdd(offset + 1);
                ctx.BindingsAdd(limit + offset);

                writer.Append("ROWS ? TO ?");
                return writer;
            }

            return null;
        }


        protected override string CompileColumns(SqlResult ctx, Query query, Writer writer)
        {
            var compiled = base.CompileColumns(ctx, query, writer.Sub());

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                ctx.PrependOne(limit);

                query.RemoveComponent("limit");

                writer.Append("SELECT FIRST ?");
                writer.Append(compiled.Substring(6));
                return writer;
            }

            if (limit == 0 && offset > 0)
            {
                ctx.PrependOne(offset);

                query.RemoveComponent("offset");

                writer.Append("SELECT SKIP ?");
                writer.Append(compiled.Substring(6));
                return writer;
            }

            writer.Append(compiled);
            return writer;
        }

        protected override void CompileBasicDateCondition(SqlResult ctx, Query query, BasicDateCondition x,
            Writer writer)
        {
            if (x.IsNot)
                writer.Append("NOT (");
            if (x.Part == "time")
            {
                writer.Append("CAST(");
                writer.AppendName(x.Column);
                writer.Append(" as TIME) ");
            }
            else if (x.Part == "date")
            {
                writer.Append("CAST(");
                writer.AppendName(x.Column);
                writer.Append(" as DATE) ");
            }
            else
            {
                writer.Append("EXTRACT(");
                writer.AppendName(x.Part.ToUpperInvariant());
                writer.Append(" FROM ");
                writer.AppendName(x.Column);
                writer.Append(") ");
            }
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" ");
            writer.Append(Parameter(ctx, query, writer, x.Value));
            if (x.IsNot)
                writer.Append(")");
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
