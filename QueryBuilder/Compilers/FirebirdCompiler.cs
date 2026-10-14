using System;
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
        protected override string SingleRowDummyTableName => "RDB$DATABASE";

        public override void VisitInsertQuery(CompileContext ctx, Query query)
        {
            var before = ctx.Buffer.Length;
            base.VisitInsertQuery(ctx, query);
            var sql = ctx.Buffer.ToString(before, ctx.Buffer.Length - before);
            ctx.Buffer.Length = before;

            var inserts = query.InsertClauses.ForEngine(EngineCode).ToList();

            if (inserts.Count > 1)
            {
                sql = Regex.Replace(sql, @"\)\s+VALUES\s+\(", ") SELECT ");
                sql = Regex.Replace(sql, @"\),\s*\(", " FROM RDB$DATABASE UNION ALL SELECT ");
                sql = Regex.Replace(sql, @"\)$", " FROM RDB$DATABASE");
            }

            ctx.Write(sql);
        }

        public override void VisitLimit(CompileContext ctx, Query query)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit > 0 && offset > 0)
            {
                ctx.EnsureSpace();
                ctx.Bindings.Add(offset + 1);
                ctx.Bindings.Add(limit + offset);

                ctx.Write($"ROWS {parameterPlaceholder} TO {parameterPlaceholder}");
            }
        }

        public override void VisitSelectPrefix(CompileContext ctx, Query query)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);
            var distinct = query.IsDistinct ? "DISTINCT " : "";

            if (limit > 0 && offset == 0)
            {
                ctx.Bindings.Add(limit);
                ctx.Write($"SELECT FIRST {parameterPlaceholder} {distinct}");
                return;
            }

            if (limit == 0 && offset > 0)
            {
                ctx.Bindings.Add(offset);
                ctx.Write($"SELECT SKIP {parameterPlaceholder} {distinct}");
                return;
            }

            base.VisitSelectPrefix(ctx, query);
        }

        protected override void VisitBasicDateCondition(CompileContext ctx, BasicDateCondition condition)
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
                left = $"EXTRACT({condition.Part.ToUpperInvariant()} FROM {column})";
            }

            if (condition.IsNot)
            {
                ctx.Write("NOT (");
            }

            ctx.Write(left).Write(" ").Write(condition.Operator).Write(" ").Write(Parameter(ctx, condition.Value));

            if (condition.IsNot)
            {
                ctx.Write(")");
            }
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
