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

        public override string CompileTableExpression(SqlResult ctx, AbstractFrom from)
        {
            if (from is RawFromClause raw)
            {
                ctx.Bindings.AddRange(raw.Bindings);
                return WrapIdentifiers(raw.Expression);
            }

            if (from is QueryFromClause queryFromClause)
            {
                var fromQuery = queryFromClause.Query;

                var alias = string.IsNullOrEmpty(fromQuery.QueryAlias) ? "" : $" {TableAsKeyword}" + WrapValue(fromQuery.QueryAlias);

                var subCtx = CompileSelectQuery(fromQuery);

                ctx.Bindings.AddRange(subCtx.Bindings);

                if (!string.IsNullOrWhiteSpace(fromQuery.IndexHint))
                {
                    subCtx.RawSql += $" USE INDEX({fromQuery.IndexHint})";
                }

                return "(" + subCtx.RawSql + ")" + alias;
            }

            if (from is FromClause fromClause)
            {
                var fromStatment = Wrap(fromClause.Table);

                if (!string.IsNullOrWhiteSpace(fromClause.IndexHint))
                {
                    fromStatment += $" USE INDEX({fromClause.IndexHint})";
                }

                return fromStatment;
            }

            throw InvalidClauseException("TableExpression", from);
        }

        public override string CompileJoin(SqlResult ctx, Join join, bool isNested = false)
        {
            var from = join.GetOneComponent<AbstractFrom>("from", EngineCode);
            var conditions = join.GetComponents<AbstractCondition>("where", EngineCode);

            var joinTable = CompileTableExpression(ctx, from);
            var constraints = CompileConditions(ctx, conditions);

            var onClause = conditions.Any() ? $" ON {constraints}" : "";

            var indexHint = "";

            if (!string.IsNullOrWhiteSpace(join.IndexHint))
            {
                indexHint = $" USE INDEX({join.IndexHint})";
            }

            return $"{join.Type} {joinTable}{indexHint}{onClause}";
        }

        public override string CompileLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);


            if (offset == 0 && limit == 0)
            {
                return null;
            }

            if (offset == 0)
            {
                ctx.Bindings.Add(limit);
                return $"LIMIT {parameterPlaceholder}";
            }

            if (limit == 0)
            {

                // MySql will not accept offset without limit, so we will put a large number
                // to avoid this error.

                ctx.Bindings.Add(offset);
                return $"LIMIT 18446744073709551615 OFFSET {parameterPlaceholder}";
            }

            // We have both values

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);

            return $"LIMIT {parameterPlaceholder} OFFSET {parameterPlaceholder}";

        }
    }
}
