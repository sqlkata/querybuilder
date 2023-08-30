namespace SqlKata.Compilers
{
    public partial class Compiler
    {
        private static readonly string[] LikeOperators = { "starts", "ends", "contains", "like" };

        private void CompileCondition(SqlResult ctx, AbstractCondition clause, Writer writer)
        {
            switch (clause)
            {
                case BasicDateCondition basicDateCondition:
                    CompileBasicDateCondition(ctx, basicDateCondition, writer);
                    break;
                case BasicStringCondition basicStringCondition:
                    CompileBasicStringCondition(ctx, basicStringCondition, writer);
                    break;
                case BasicCondition basicCondition:
                    CompileBasicCondition(ctx, basicCondition, writer);
                    break;
                case BetweenCondition betweenCondition:
                    CompileBetweenCondition(ctx, betweenCondition, writer);
                    break;
                case BooleanCondition booleanCondition:
                    CompileBooleanCondition(booleanCondition, writer);
                    break;
                case ExistsCondition existsCondition:
                    CompileExistsCondition(ctx, existsCondition, writer);
                    break;
                case InCondition inCondition:
                    CompileInCondition(ctx, inCondition, writer);
                    break;
                case InQueryCondition inQueryCondition:
                    CompileInQueryCondition(ctx, inQueryCondition, writer);
                    break;
                case NestedCondition nestedCondition:
                    CompileNestedCondition(ctx, nestedCondition, writer);
                    break;
                case NullCondition nullCondition:
                    CompileNullCondition(nullCondition, writer);
                    break;
                case QueryCondition queryCondition:
                    CompileQueryCondition(ctx, queryCondition, writer);
                    break;
                case RawCondition rawCondition:
                    CompileRawCondition(ctx, rawCondition, writer);
                    break;
                case SubQueryCondition subQueryCondition:
                    CompileSubQueryCondition(ctx, subQueryCondition, writer);
                    break;
                case TwoColumnsCondition twoColumnsCondition:
                    CompileTwoColumnsCondition(twoColumnsCondition, writer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clause));
            }
        }

        private void CompileConditions(SqlResult ctx, List<AbstractCondition> conditions, Writer writer)
        {
            writer.List(" ", conditions, (c, i) =>
            {
                if (i != 0)
                    writer.S.Append(c.IsOr ? "OR " : "AND ");
                CompileCondition(ctx, c, writer);
            });
        }

        private void CompileRawCondition(SqlResult ctx, RawCondition x, Writer writer)
        {
            ctx.Bindings.AddRange(x.Bindings);
            writer.BindMany(x.Bindings);
            writer.AppendRaw(x.Expression);
        }

        private void CompileQueryCondition(SqlResult ctx, QueryCondition x, Writer writer)
        {
            var subCtx = CompileSelectQuery(x.Query, new Writer(XService));

            ctx.Bindings.AddRange(subCtx.Bindings);

            writer.AppendName(x.Column);
            writer.S.Append(" ");
            writer.S.Append(Operators.CheckOperator(x.Operator));
            writer.S.Append(" (");
            writer.S.Append(subCtx.RawSql);
            writer.S.Append(")");
        }

        private void CompileSubQueryCondition(SqlResult ctx, SubQueryCondition x, Writer writer)
        {
            writer.S.Append("(");
            var subCtx = CompileSelectQuery(x.Query, writer);
            ctx.Bindings.AddRange(subCtx.Bindings);
            writer.S.Append(") ");
            writer.S.Append(Operators.CheckOperator(x.Operator));
            writer.S.Append(" ");
            writer.S.Append(Parameter(ctx, x.Value));
        }

        private void CompileBasicCondition(SqlResult ctx, BasicCondition x, Writer writer)
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
        }

        protected virtual void CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x, Writer writer)
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
        }

        protected virtual void CompileBasicDateCondition(SqlResult ctx, BasicDateCondition x, Writer writer)
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
        }

        private void CompileNestedCondition(SqlResult ctx, NestedCondition x, Writer writer)
        {
            if (!x.Query.HasComponent("where", EngineCode) &&
                !x.Query.HasComponent("having", EngineCode))
                return;

            var clause = x.Query.HasComponent("where", EngineCode) ? "where" : "having";

            var clauses = x.Query.GetComponents<AbstractCondition>(clause, EngineCode);

            if (x.IsNot)
                writer.S.Append("NOT ");
            writer.S.Append("(");
            CompileConditions(ctx, clauses, writer);
            writer.S.Append(")");

        }

        private void CompileTwoColumnsCondition(TwoColumnsCondition x, Writer writer)
        {
            if (x.IsNot)
                writer.S.Append("NOT ");
            writer.AppendName(x.First);
            writer.S.Append(" ");
            writer.S.Append(Operators.CheckOperator(x.Operator));
            writer.S.Append(" ");
            writer.AppendName(x.Second);
        }

        private void CompileBetweenCondition(SqlResult ctx, BetweenCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.S.Append(x.IsNot ? " NOT BETWEEN " : " BETWEEN ");
            writer.S.Append(Parameter(ctx, x.Lower));
            writer.S.Append(" AND ");
            writer.S.Append(Parameter(ctx, x.Higher));
        }

        private void CompileInCondition(SqlResult ctx, InCondition x, Writer writer)
        {
            if (!x.Values.Any())
            {
                writer.S.Append(x.IsNot ? "1 = 1 /* NOT IN [empty list] */" : "1 = 0 /* IN [empty list] */");
                return;
            }

            writer.AppendName(x.Column);
            writer.S.Append(x.IsNot ? " NOT IN (" : " IN (");
            writer.S.Append(Parametrize(ctx, x.Values.OfType<object>()));
            writer.S.Append(")");
        }

        private void CompileInQueryCondition(SqlResult ctx, InQueryCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.S.Append(x.IsNot ? " NOT IN (" : " IN (");
            var subCtx = CompileSelectQuery(x.Query, writer.Sub());
            ctx.Bindings.AddRange(subCtx.Bindings);
            writer.S.Append(subCtx.RawSql);
            writer.S.Append(")");
        }

        private void CompileNullCondition(NullCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.S.Append(x.IsNot ? " IS NOT NULL" : " IS NULL");
        }

        private void CompileBooleanCondition(BooleanCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.S.Append(x.IsNot ? " != " : " = ");
            writer.S.Append(x.Value ? CompileTrue() : CompileFalse());
        }

        private void CompileExistsCondition(SqlResult ctx, ExistsCondition item, Writer writer)
        {
            writer.S.Append(item.IsNot ? "NOT EXISTS (" : "EXISTS (");

            var query = OmitSelectInsideExists
                ? item.Query.Clone().RemoveComponent("select").SelectRaw("1")
                : item.Query;

            var subCtx = CompileSelectQuery(query, writer.Sub());
            ctx.Bindings.AddRange(subCtx.Bindings);
            writer.S.Append(subCtx.RawSql);

            writer.S.Append(")");
        }
    }
}
