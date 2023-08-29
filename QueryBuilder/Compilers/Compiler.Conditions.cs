namespace SqlKata.Compilers
{
    public partial class Compiler
    {
        private static readonly string[] LikeOperators = { "starts", "ends", "contains", "like" };

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
            writer.Bindings.AddRange(x.Bindings);
            writer.AppendRaw(x.Expression);
            return writer;
        }

        private string CompileQueryCondition(SqlResult ctx, QueryCondition x, Writer writer)
        {
            var subCtx = CompileSelectQuery(x.Query, new Writer(XService));

            ctx.Bindings.AddRange(subCtx.Bindings);

            writer.AppendName(x.Column);
            writer.S.Append(" ");
            writer.S.Append(Operators.CheckOperator(x.Operator));
            writer.S.Append(" (");
            writer.S.Append(subCtx.RawSql);
            writer.S.Append(")");
            return writer;
        }

        private string CompileSubQueryCondition(SqlResult ctx, SubQueryCondition x, Writer writer)
        {
            writer.S.Append("(");
            var subCtx = CompileSelectQuery(x.Query, writer);
            ctx.Bindings.AddRange(subCtx.Bindings);
            writer.S.Append(") ");
            writer.S.Append(Operators.CheckOperator(x.Operator));
            writer.S.Append(" ");
            writer.S.Append(Parameter(ctx, x.Value));
            return writer;
        }

        private string CompileBasicCondition(SqlResult ctx, BasicCondition x, Writer writer)
        {
            if (x.IsNot)
                writer.S.Append("NOT (");
            writer.AppendName(x.Column);
            writer.S.Append(" ");
            writer.S.Append(Operators.CheckOperator(x.Operator));
            writer.S.Append(" ");
            writer.S.Append(Parameter(ctx, x.Value));
            if (x.IsNot)
                writer.S.Append(")");

            return writer;
        }

        protected virtual string CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x, Writer writer)
        {
            var column = XService.Wrap(x.Column);

            if (Resolve(ctx, x.Value) is not string value)
                throw new ArgumentException("Expecting a non nullable string");

            var method = x.Operator;

            if (LikeOperators.Contains(x.Operator))
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


            if (!x.CaseSensitive)
            {
                column = $"LOWER({column})";
                value = value.ToLowerInvariant();
            }

            if (x.IsNot)
                writer.S.Append("NOT (");
            writer.S.Append(column);
            writer.S.Append(" ");
            writer.S.Append(Operators.CheckOperator(method));
            writer.S.Append(" ");
            writer.S.Append(x.Value is UnsafeLiteral ? value : Parameter(ctx, value));
            if (x.EscapeCharacter is { } esc1)
            {
                writer.S.Append(" ESCAPE '");
                writer.S.Append(esc1);
                writer.S.Append('\'');
            }

            if (x.IsNot)
                writer.S.Append(")");
            return writer;
        }

        protected virtual string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition x, Writer writer)
        {
            if (x.IsNot)
                writer.S.Append("NOT (");
            writer.S.Append(x.Part.ToUpperInvariant());
            writer.S.Append("(");
            writer.AppendName(x.Column);
            writer.S.Append(") ");
            writer.S.Append(Operators.CheckOperator(x.Operator));
            writer.S.Append(" ");
            writer.S.Append(Parameter(ctx, x.Value));
            if (x.IsNot)
                writer.S.Append(")");
            return writer;
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

        private string CompileTwoColumnsCondition(TwoColumnsCondition x, Writer writer)
        {
            if (x.IsNot)
                writer.S.Append("NOT ");
            writer.AppendName(x.First);
            writer.S.Append(" ");
            writer.S.Append(Operators.CheckOperator(x.Operator));
            writer.S.Append(" ");
            writer.AppendName(x.Second);
            return writer;
        }

        private string CompileBetweenCondition(SqlResult ctx, BetweenCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.S.Append(x.IsNot ? " NOT BETWEEN " : " BETWEEN ");
            writer.S.Append(Parameter(ctx, x.Lower));
            writer.S.Append(" AND ");
            writer.S.Append(Parameter(ctx, x.Higher));
            return writer;
        }

        private string CompileInCondition(SqlResult ctx, InCondition x, Writer writer)
        {
            if (!x.Values.Any())
            {
                writer.S.Append(x.IsNot ? "1 = 1 /* NOT IN [empty list] */" : "1 = 0 /* IN [empty list] */");
                return writer;
            }

            writer.AppendName(x.Column);
            writer.S.Append(x.IsNot ? " NOT IN (" : " IN (");
            writer.S.Append(Parametrize(ctx, x.Values.OfType<object>()));
            writer.S.Append(")");
            return writer;
        }

        private string CompileInQueryCondition(SqlResult ctx, InQueryCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.S.Append(x.IsNot ? " NOT IN (" : " IN (");
            var subCtx = CompileSelectQuery(x.Query, writer.Sub());
            ctx.Bindings.AddRange(subCtx.Bindings);
            writer.S.Append(subCtx.RawSql);
            writer.S.Append(")");
            return writer;
        }

        private string CompileNullCondition(NullCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.S.Append(x.IsNot ? " IS NOT NULL" : " IS NULL");
            return writer;
        }

        private string CompileBooleanCondition(BooleanCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.S.Append(x.IsNot ? " != " : " = ");
            writer.S.Append(x.Value ? CompileTrue() : CompileFalse());
            return writer;
        }

        private string CompileExistsCondition(SqlResult ctx, ExistsCondition item, Writer writer)
        {
            writer.S.Append(item.IsNot ? "NOT EXISTS (" : "EXISTS (");

            var query = OmitSelectInsideExists
                ? item.Query.Clone().RemoveComponent("select").SelectRaw("1")
                : item.Query;

            var subCtx = CompileSelectQuery(query, writer.Sub());
            ctx.Bindings.AddRange(subCtx.Bindings);
            writer.S.Append(subCtx.RawSql);

            writer.S.Append(")");
            return writer;
        }
    }
}
