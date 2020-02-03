using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlKata.Compilers
{
    public class SqlAnywhereCompiler : Compiler
    {
        private string _trueValue, _falseValue;

        public SqlAnywhereCompiler(string trueValue, string falseValue)
        {
            LastId = "SELECT @@identity as Id";
            parameterPrefix = ":p";

            // There is no explicit implementation of boolean.
            // It could be done using 1/0 or Yes/No or whatever
            _trueValue = trueValue;
            _falseValue = falseValue;
        }

        public override string EngineCode { get; } = EngineCodes.SqlAnywhere;

        protected override SqlResult CompileSelectQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query.Clone(),
            };

            var results = new[] {
                    this.CompileColumns(ctx),
                    this.CompileFrom(ctx),
                    this.CompileJoins(ctx),
                    this.CompileWheres(ctx),
                    this.CompileGroups(ctx),
                    this.CompileHaving(ctx),
                    this.CompileOrders(ctx),
                    //this.CompileLimit(ctx),// Moved inside CompileColumns (SqlAnywhere uses TOP / START AT instead of LIMIT / OFFSET)
                    this.CompileUnion(ctx),
                }
               .Where(x => x != null)
               .Where(x => !string.IsNullOrEmpty(x))
               .ToList();

            string sql = string.Join(" ", results);

            ctx.RawSql = sql;

            return ctx;
        }

        public override string CompileLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return null;
            }

            if (offset == 0)
            {
                ctx.Bindings.Add(limit);
                return "TOP ?";
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                return "TOP ALL START AT ?";
            }

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);

            return "TOP ? START AT ?";
        }

        protected override string CompileColumns(SqlResult ctx)
        {
            var compiled = base.CompileColumns(ctx);

            // If there is a limit or an offset on the query, we will add the top
            // clause to the query, which serves as a "limit" type clause within the
            // SQL Anywhere system similar to the limit keywords available in MySQL.
            var limit = CompileLimit(ctx);

            if (!string.IsNullOrWhiteSpace(limit))
            {
                // handle distinct
                if (compiled.IndexOf("SELECT DISTINCT") == 0)
                {
                    return "SELECT DISTINCT " + limit + compiled.Substring(15);
                }

                return "SELECT " + limit + compiled.Substring(6);
            }

            return compiled;
        }

        public override string CompileRandom(string seed)
        {
            if (string.IsNullOrEmpty(seed))
            {
                return "RAND()";
            }
            else
            {
                int numSeed;
                if (int.TryParse(seed, out numSeed))
                {
                    return $"RAND({numSeed})";
                }
                else
                {
                    return "RAND()";
                }
            }
        }


        public override string CompileTrue()
        {
            return _trueValue;
        }

        public override string CompileFalse()
        {
            return _falseValue;
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            if (condition.Part != "time")
            {
                return base.CompileBasicDateCondition(ctx, condition);
            }
            
            var column = Wrap(condition.Column);

            string left;
            string sql;
            if (condition.Value is DateTime || condition.Value is TimeSpan)
            {
                const string format = @"hh\:mm\:ss\:ffffff";
                string value;
                if (condition.Value is DateTime)
                {
                    value = ((DateTime)condition.Value).ToString(format);
                }
                else
                {
                    value = ((TimeSpan)condition.Value).ToString(format);
                }
                condition.Value = value;
            }

            left = $"CAST({column} as TIME)";

            sql = $"{left} {condition.Operator} CAST({Parameter(ctx, condition.Value)} as TIME)";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

    }
}
