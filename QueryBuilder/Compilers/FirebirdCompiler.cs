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
            var ctx = base.CompileInsertQuery(query);

            var inserts = ctx.Query.GetComponents<AbstractInsertClause>("insert", EngineCode);

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
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset > 0)
            {
                ctx.Bindings.Add(offset + 1);
                ctx.Bindings.Add(limit + offset);

                return "ROWS ? TO ?";
            }

            return null;
        }

        public override string CompileUnion(SqlResult ctx)
        {
            // Handle UNION, EXCEPT and INTERSECT
            if (!ctx.Query.GetComponents("combine", EngineCode).Any())
            {
                return null;
            }

            var combinedQueries = new List<string>();

            var clauses = ctx.Query.GetComponents<AbstractCombine>("combine", EngineCode);

            foreach (var clause in clauses)
            {
                if (clause is Combine combineClause)
                {
                    var combineOperator = combineClause.Operation.ToUpper() + " " + (combineClause.All ? "ALL " : "");

                    var subCtx = CompileSelectQuery(combineClause.Query);

                    ctx.Bindings.AddRange(subCtx.Bindings);

                    combinedQueries.Add($"{combineOperator}{subCtx.RawSql}");
                }
                else
                {
                    var combineRawClause = clause as RawCombine;

                    ctx.Bindings.AddRange(combineRawClause.Bindings);

                    combinedQueries.Add(WrapIdentifiers(combineRawClause.Expression));

                }
            }

            return string.Join(" ", combinedQueries);
        }

        protected override string CompileColumns(SqlResult ctx)
        {
            var compiled = base.CompileColumns(ctx);

            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

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
            var column = Wrap(condition.Column);

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
                left = $"EXTRACT({condition.Part.ToUpper()} FROM {column})";
            }

            var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        public override string WrapValue(string value)
        {
            return base.WrapValue(value).ToUpper();
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
