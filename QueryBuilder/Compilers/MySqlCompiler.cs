using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata.Compilers
{
    public class MySqlCompiler : Compiler
    {
        public MySqlCompiler()
        {
            OpeningIdentifier = ClosingIdentifier = "`";
            LastId = "SELECT last_insert_id() as Id";
        }

        public override string EngineCode { get; } = EngineCodes.MySql;

        public override void VisitUpdateQuery(CompileContext ctx, Query query)
        {
            var hasJoins = query.Joins.ForEngine(EngineCode).Any();
            if (!hasJoins)
            {
                base.VisitUpdateQuery(ctx, query);
                return;
            }

            var fromClause = query.FromClauses.ForEngineOrDefault(EngineCode);

            if (fromClause is null)
            {
                throw new InvalidOperationException("No table set to update");
            }

            string table = null;

            if (fromClause is FromClause fromClauseCast)
            {
                table = Wrap(fromClauseCast.Table);
            }

            RawFromClause rawFrom = null;
            if (fromClause is RawFromClause rawFromClause)
            {
                rawFrom = rawFromClause;
                table = WrapIdentifiers(rawFromClause.Expression);
            }

            if (table is null)
            {
                throw new InvalidOperationException("Invalid table expression");
            }

            var clause = query.UpdateClauses.ForEngineOrDefault(EngineCode);

            if (clause != null && clause is IncrementClause increment)
            {
                ctx.Write("UPDATE ");
                if (rawFrom != null)
                {
                    VisitRaw(ctx, rawFrom.Expression, rawFrom.Bindings);
                }
                else
                {
                    ctx.Write(table);
                }
                VisitJoins(ctx, query);
                var column = Wrap(increment.Column);
                var value = Parameter(ctx, Math.Abs(increment.Value));
                var sign = increment.Value >= 0 ? "+" : "-";

                ctx.Write($" SET {column} = {column} {sign} {value}");
                VisitWheres(ctx, query);
                return;
            }

            ctx.Write("UPDATE ");
            if (rawFrom != null)
            {
                VisitRaw(ctx, rawFrom.Expression, rawFrom.Bindings);
            }
            else
            {
                ctx.Write(table);
            }
            VisitJoins(ctx, query);

            var toUpdate = clause as InsertClause;
            var parts = new List<string>();

            for (var i = 0; i < toUpdate.Columns.Count; i++)
            {
                parts.Add(Wrap(toUpdate.Columns[i]) + " = " + Parameter(ctx, toUpdate.Values[i]));
            }

            var sets = string.Join(", ", parts);

            ctx.Write($" SET {sets}");
            VisitWheres(ctx, query);
        }

        public override void VisitLimit(CompileContext ctx, Query query)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (offset == 0 && limit == 0)
            {
                return;
            }

            ctx.EnsureSpace();

            if (offset == 0)
            {
                ctx.Write($"LIMIT {Parameter(ctx, limit)}");
                return;
            }

            if (limit == 0)
            {
                ctx.Write($"LIMIT 18446744073709551615 OFFSET {Parameter(ctx, offset)}");
                return;
            }

            ctx.Write($"LIMIT {Parameter(ctx, limit)} OFFSET {Parameter(ctx, offset)}");
        }

        public override string CompileRandom(int? seed)
        {
            return seed.HasValue ? $"RAND({seed})" : "RAND()";
        }
    }
}
