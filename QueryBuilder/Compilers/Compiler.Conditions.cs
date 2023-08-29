namespace SqlKata.Compilers
{
    public partial class Compiler
    {
        private string? CompileCondition(SqlResult ctx, AbstractCondition clause, Writer writer)
        {
            return clause switch
            {
                BasicDateCondition basicDateCondition => CompileBasicDateCondition(ctx, basicDateCondition, writer),
                BasicStringCondition basicStringCondition => CompileBasicStringCondition(ctx, basicStringCondition, writer),
                BasicCondition basicCondition => CompileBasicCondition(ctx, basicCondition, writer),
                BetweenCondition betweenCondition => CompileBetweenCondition(ctx, betweenCondition, writer),
                BooleanCondition booleanCondition => CompileBooleanCondition(booleanCondition, writer),
                ExistsCondition existsCondition => CompileExistsCondition(ctx, existsCondition, writer),
                InCondition inCondition => CompileInCondition(ctx, inCondition, writer),
                InQueryCondition inQueryCondition => CompileInQueryCondition(ctx, inQueryCondition, writer),
                NestedCondition nestedCondition => CompileNestedCondition(ctx, nestedCondition, writer),
                NullCondition nullCondition => CompileNullCondition(nullCondition, writer),
                QueryCondition queryCondition => CompileQueryCondition(ctx, queryCondition, writer),
                RawCondition rawCondition => CompileRawCondition(ctx, rawCondition, writer),
                SubQueryCondition subQueryCondition => CompileSubQueryCondition(ctx, subQueryCondition, writer),
                TwoColumnsCondition twoColumnsCondition => CompileTwoColumnsCondition(twoColumnsCondition, writer),
                _ => throw new ArgumentOutOfRangeException(nameof(clause))
            };
        }

        private string CompileConditions(SqlResult ctx, List<AbstractCondition> conditions, Writer writer)
        {
            var result = new List<string>();

            for (var i = 0; i < conditions.Count; i++)
            {
                var compiled = CompileCondition(ctx, conditions[i], writer.Sub());

                if (string.IsNullOrEmpty(compiled)) continue;

                var boolOperator = i == 0 ? "" : conditions[i].IsOr ? "OR " : "AND ";

                result.Add(boolOperator + compiled);
            }
            writer.List(" ", result);
            return writer;
        }

        private string CompileRawCondition(SqlResult ctx, RawCondition x, Writer writer)
        {
            ctx.Bindings.AddRange(x.Bindings);
            return XService.WrapIdentifiers(x.Expression);
        }

        private string CompileQueryCondition(SqlResult ctx, QueryCondition x, Writer writer)
        {
            var subCtx = CompileSelectQuery(x.Query, new Writer(XService));

            ctx.Bindings.AddRange(subCtx.Bindings);

            return XService.Wrap(x.Column) + " " + Operators.CheckOperator(x.Operator) + " (" + subCtx.RawSql + ")";
        }

        private string CompileSubQueryCondition(SqlResult ctx, SubQueryCondition x, Writer writer)
        {
            var subCtx = CompileSelectQuery(x.Query, new Writer(XService));

            ctx.Bindings.AddRange(subCtx.Bindings);

            return "(" + subCtx.RawSql + ") " + Operators.CheckOperator(x.Operator) + " " + Parameter(ctx, x.Value);
        }

        private string CompileBasicCondition(SqlResult ctx, BasicCondition x, Writer writer)
        {
            var sql = $"{XService.Wrap(x.Column)} {Operators.CheckOperator(x.Operator)} {Parameter(ctx, x.Value)}";

            if (x.IsNot) return $"NOT ({sql})";

            return sql;
        }

        protected virtual string CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x, Writer writer)
        {
            var column = XService.Wrap(x.Column);

            if (Resolve(ctx, x.Value) is not string value)
                throw new ArgumentException("Expecting a non nullable string");

            var method = x.Operator;

            if (new[] { "starts", "ends", "contains", "like" }.Contains(x.Operator))
            {
                method = "LIKE";

                switch (x.Operator)
                {
                    case "starts":
                        value = $"{value}%";
                        break;
                    case "ends":
                        value = $"%{value}";
                        break;
                    case "contains":
                        value = $"%{value}%";
                        break;
                }
            }

            string sql;


            if (!x.CaseSensitive)
            {
                column = CompileLower(column);
                value = value.ToLowerInvariant();
            }

            sql = x.Value is UnsafeLiteral
                ? $"{column} {Operators.CheckOperator(method)} {value}"
                : $"{column} {Operators.CheckOperator(method)} {Parameter(ctx, value)}";

            if (x.EscapeCharacter is { } esc) sql = $"{sql} ESCAPE '{esc}'";

            return x.IsNot ? $"NOT ({sql})" : sql;
        }

        protected virtual string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition x, Writer writer)
        {
            var column = XService.Wrap(x.Column);
            var op = Operators.CheckOperator(x.Operator);

            var sql = $"{x.Part.ToUpperInvariant()}({column}) {op} {Parameter(ctx, x.Value)}";

            return x.IsNot ? $"NOT ({sql})" : sql;
        }

        private string? CompileNestedCondition(SqlResult ctx, NestedCondition x, Writer writer)
        {
            if (!x.Query.HasComponent("where", EngineCode) &&
                !x.Query.HasComponent("having", EngineCode))
                return null;

            var clause = x.Query.HasComponent("where", EngineCode) ? "where" : "having";

            var clauses = x.Query.GetComponents<AbstractCondition>(clause, EngineCode);

            var sql = CompileConditions(ctx, clauses, new Writer(XService));

            return x.IsNot ? $"NOT ({sql})" : $"({sql})";
        }

        private string CompileTwoColumnsCondition(TwoColumnsCondition clause, Writer writer)
        {
            var op = clause.IsNot ? "NOT " : "";
            return $"{op}{XService.Wrap(clause.First)} {Operators.CheckOperator(clause.Operator)} {XService.Wrap(clause.Second)}";
        }

        private string CompileBetweenCondition(SqlResult ctx, BetweenCondition item, Writer writer)
        {
            var between = item.IsNot ? "NOT BETWEEN" : "BETWEEN";
            var lower = Parameter(ctx, item.Lower);
            var higher = Parameter(ctx, item.Higher);

            return XService.Wrap(item.Column) + $" {between} {lower} AND {higher}";
        }

        private string CompileInCondition(SqlResult ctx, InCondition item, Writer writer)
        {
            var column = XService.Wrap(item.Column);

            if (!item.Values.Any())
                return item.IsNot ? "1 = 1 /* NOT IN [empty list] */" : "1 = 0 /* IN [empty list] */";

            var inOperator = item.IsNot ? "NOT IN" : "IN";

            var values = Parametrize(ctx, item.Values.OfType<object>());

            return column + $" {inOperator} ({values})";
        }

        private string CompileInQueryCondition(SqlResult ctx, InQueryCondition item, Writer writer)
        {
            var subCtx = CompileSelectQuery(item.Query, new Writer(XService));

            ctx.Bindings.AddRange(subCtx.Bindings);

            var inOperator = item.IsNot ? "NOT IN" : "IN";

            return XService.Wrap(item.Column) + $" {inOperator} ({subCtx.RawSql})";
        }

        private string CompileNullCondition(NullCondition item, Writer writer)
        {
            var op = item.IsNot ? "IS NOT NULL" : "IS NULL";
            return XService.Wrap(item.Column) + " " + op;
        }

        private string CompileBooleanCondition(BooleanCondition item, Writer writer)
        {
            var column = XService.Wrap(item.Column);

            var op = item.IsNot ? "!=" : "=";
            var value = item.Value ? CompileTrue() : CompileFalse();

            return $"{column} {op} {value}";
        }

        private string CompileExistsCondition(SqlResult ctx, ExistsCondition item, Writer writer)
        {
            var op = item.IsNot ? "NOT EXISTS" : "EXISTS";


            // remove unneeded components
            var query = item.Query.Clone();

            if (OmitSelectInsideExists) query.RemoveComponent("select").SelectRaw("1");

            var subCtx = CompileSelectQuery(query, new Writer(XService));

            ctx.Bindings.AddRange(subCtx.Bindings);

            return $"{op} ({subCtx.RawSql})";
        }
    }
}
