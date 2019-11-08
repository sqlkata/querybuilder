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
            SqlResult context = base.CompileInsertQuery(query);

            List<AbstractInsertClause> inserts = context.Query.GetComponents<AbstractInsertClause>("insert", EngineCode);

            if (inserts.Count > 1)
            {
                context.RawSql = Regex.Replace(context.RawSql, @"\)\s+VALUES\s+\(", ") SELECT ");
                context.RawSql = Regex.Replace(context.RawSql, @"\),\s*\(", " FROM RDB$DATABASE UNION ALL SELECT ");
                context.RawSql = Regex.Replace(context.RawSql, @"\)$", " FROM RDB$DATABASE");
            }

            return context;
        }

        public override string CompileLimit(SqlResult context)
        {
            int limit = context.Query.GetLimit(EngineCode);
            int offset = context.Query.GetOffset(EngineCode);

            if (limit > 0 && offset > 0)
            {
                context.Bindings.Add(offset + 1);
                context.Bindings.Add(limit + offset);

                return "ROWS ? TO ?";
            }

            return null;
        }


        protected override string CompileColumns(SqlResult context)
        {
            string compiled = base.CompileColumns(context);

            int limit = context.Query.GetLimit(EngineCode);
            int offset = context.Query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                context.Bindings.Insert(0, limit);

                context.Query.ClearComponent("limit");

                return "SELECT FIRST ?" + compiled.Substring(6);
            }
            else if (limit == 0 && offset > 0)
            {
                context.Bindings.Insert(0, offset);

                context.Query.ClearComponent("offset");

                return "SELECT SKIP ?" + compiled.Substring(6);
            }

            return compiled;
        }

        protected override string CompileBasicDateCondition(SqlResult context, BasicDateCondition condition)
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

            string sql = $"{left} {condition.Operator} {Parameter(context, condition.Value)}";

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
