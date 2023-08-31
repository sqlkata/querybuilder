namespace SqlKata.Compilers
{
    public partial class Compiler
    {
        private static readonly string[] LikeOperators = { "starts", "ends", "contains", "like" };

        private void CompileCondition(Query query, AbstractCondition clause, Writer writer)
        {
            switch (clause)
            {
                case BasicDateCondition basicDateCondition:
                    CompileBasicDateCondition(query, basicDateCondition, writer);
                    break;
                case BasicStringCondition basicStringCondition:
                    CompileBasicStringCondition(query, basicStringCondition, writer);
                    break;
                case BasicCondition basicCondition:
                    CompileBasicCondition(query, basicCondition, writer);
                    break;
                case BetweenCondition betweenCondition:
                    CompileBetweenCondition(query, betweenCondition, writer);
                    break;
                case BooleanCondition booleanCondition:
                    CompileBooleanCondition(booleanCondition, writer);
                    break;
                case ExistsCondition existsCondition:
                    CompileExistsCondition(existsCondition, writer);
                    break;
                case InCondition inCondition:
                    CompileInCondition(query, inCondition, writer);
                    break;
                case InQueryCondition inQueryCondition:
                    CompileInQueryCondition(inQueryCondition, writer);
                    break;
                case NestedCondition nestedCondition:
                    CompileNestedCondition(query, nestedCondition, writer);
                    break;
                case NullCondition nullCondition:
                    CompileNullCondition(nullCondition, writer);
                    break;
                case QueryCondition queryCondition:
                    CompileQueryCondition(queryCondition, writer);
                    break;
                case RawCondition rawCondition:
                    CompileRawCondition(rawCondition, writer);
                    break;
                case SubQueryCondition subQueryCondition:
                    CompileSubQueryCondition(query, subQueryCondition, writer);
                    break;
                case TwoColumnsCondition twoColumnsCondition:
                    CompileTwoColumnsCondition(twoColumnsCondition, writer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clause));
            }
        }

        private void CompileConditions(Query query, List<AbstractCondition> conditions, Writer writer)
        {
            writer.List(" ", conditions, (c, i) =>
            {
                if (i != 0)
                    writer.Append(c.IsOr ? "OR " : "AND ");
                CompileCondition(query, c, writer);
            });
        }

        private void CompileRawCondition(RawCondition x, Writer writer)
        {
            writer.AppendRaw(x.Expression, x.Bindings);
        }

        private void CompileQueryCondition(QueryCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.Append(" ");
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" (");
            CompileSelectQuery(x.Query, writer);
            writer.Append(")");
        }

        private void CompileSubQueryCondition(Query query, SubQueryCondition x, Writer writer)
        {
            writer.Append("(");
            CompileSelectQuery(x.Query, writer);
            writer.Append(") ");
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" ");
            writer.AppendParameter(query, x.Value);
        }

        private void CompileBasicCondition(Query query, BasicCondition x, Writer writer)
        {
            if (x.IsNot)
                writer.Append("NOT (");
            writer.AppendName(x.Column);
            writer.Append(" ");
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" ");
            writer.AppendParameter(query, x.Value);
            if (x.IsNot)
                writer.Append(")");
        }

        protected virtual void CompileBasicStringCondition(
            Query query, BasicStringCondition x, Writer writer)
        {
            if (Resolve(query, x.Value) is not string value)
                throw new ArgumentException("Expecting a non nullable string");

            if (x.IsNot)
                writer.Append("NOT (");
            if (!x.CaseSensitive)
                writer.Append("LOWER(");
            writer.AppendName(x.Column);
            if (!x.CaseSensitive)
                writer.Append(")");

            writer.Append(" ");
            var isLikeOperator = LikeOperators.Contains(x.Operator);
            var method = isLikeOperator ? "LIKE" : x.Operator;
            writer.Append(Operators.CheckOperator(method));
            writer.Append(" ");
            if (isLikeOperator)
            {
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
            writer.AppendParameter(query,
                x.CaseSensitive ? value : value.ToLowerInvariant());
            if (x.EscapeCharacter is { } esc)
            {
                writer.Append(" ESCAPE '");
                writer.Append(esc);
                writer.Append('\'');
            }

            if (x.IsNot)
                writer.Append(")");
        }

        protected virtual void CompileBasicDateCondition(Query query, BasicDateCondition x,
            Writer writer)
        {
            if (x.IsNot)
                writer.Append("NOT (");
            writer.Append(x.Part.ToUpperInvariant());
            writer.Append("(");
            writer.AppendName(x.Column);
            writer.Append(") ");
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" ");
            writer.AppendParameter(query, x.Value);
            if (x.IsNot)
                writer.Append(")");
        }

        private void CompileNestedCondition(Query query, NestedCondition x, Writer writer)
        {
            if (!x.Query.HasComponent("where", EngineCode) &&
                !x.Query.HasComponent("having", EngineCode))
                return;

            var clause = x.Query.HasComponent("where", EngineCode) ? "where" : "having";

            var clauses = x.Query.GetComponents<AbstractCondition>(clause, EngineCode);

            if (x.IsNot)
                writer.Append("NOT ");
            writer.Append("(");
            CompileConditions(query, clauses, writer);
            writer.Append(")");
        }

        private void CompileTwoColumnsCondition(TwoColumnsCondition x, Writer writer)
        {
            if (x.IsNot)
                writer.Append("NOT ");
            writer.AppendName(x.First);
            writer.Append(" ");
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" ");
            writer.AppendName(x.Second);
        }

        private void CompileBetweenCondition(Query query, BetweenCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.Append(x.IsNot ? " NOT BETWEEN " : " BETWEEN ");
            writer.AppendParameter(query, x.Lower);
            writer.Append(" AND ");
            writer.AppendParameter(query, x.Higher);
        }

        private void CompileInCondition(Query query, InCondition x, Writer writer)
        {
            if (!x.Values.Any())
            {
                writer.Append(x.IsNot
                    ? "1 = 1 /* NOT IN [empty list] */"
                    : "1 = 0 /* IN [empty list] */");
                return;
            }

            writer.AppendName(x.Column);
            writer.Append(x.IsNot ? " NOT IN (" : " IN (");
            writer.CommaSeparatedParameters(query, x.Values.OfType<object>());
            writer.Append(")");
        }

        private void CompileInQueryCondition(InQueryCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.Append(x.IsNot ? " NOT IN (" : " IN (");
            CompileSelectQuery(x.Query, writer);
            writer.Append(")");
        }

        private void CompileNullCondition(NullCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.Append(x.IsNot ? " IS NOT NULL" : " IS NULL");
        }

        private void CompileBooleanCondition(BooleanCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.Append(x.IsNot ? " != " : " = ");
            writer.Append(x.Value ? CompileTrue() : CompileFalse());
        }

        private void CompileExistsCondition(ExistsCondition item, Writer writer)
        {
            writer.Append(item.IsNot ? "NOT EXISTS (" : "EXISTS (");

            var query = OmitSelectInsideExists
                ? item.Query.Clone().RemoveComponent("select").SelectRaw("1")
                : item.Query;

            CompileSelectQuery(query, writer);
            writer.Append(")");
        }
    }
}
