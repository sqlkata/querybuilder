using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata.Compilers
{
    public partial class Compiler
    {
        public virtual void VisitConditions(CompileContext ctx, List<AbstractCondition> conditions)
        {
            var isFirst = true;

            for (var i = 0; i < conditions.Count; i++)
            {
                var cond = conditions[i];
                var beforeLen = ctx.Buffer.Length;

                if (!isFirst)
                {
                    ctx.Write(cond.IsOr ? "OR " : "AND ");
                }

                var condStart = ctx.Buffer.Length;
                VisitCondition(ctx, cond);

                if (ctx.Buffer.Length == condStart)
                {
                    // Nothing was written for this condition, rollback operator if added
                    ctx.Buffer.Length = beforeLen;
                }
                else
                {
                    isFirst = false;
                    if (i < conditions.Count - 1)
                    {
                        ctx.Write(" ");
                    }
                }
            }

            if (ctx.Buffer.Length > 0 && ctx.Buffer[ctx.Buffer.Length - 1] == ' ')
            {
                ctx.Buffer.Length--;
            }
        }

        public virtual void VisitCondition(CompileContext ctx, AbstractCondition clause)
        {
            switch (clause)
            {
                case BasicDateCondition date:
                    VisitBasicDateCondition(ctx, date);
                    break;
                case BasicStringCondition str:
                    VisitBasicStringCondition(ctx, str);
                    break;
                case BasicCondition basic:
                    VisitBasicCondition(ctx, basic);
                    break;
                case TwoColumnsCondition two:
                    VisitTwoColumnsCondition(ctx, two);
                    break;
                case BetweenCondition between:
                    VisitBetweenCondition(ctx, between);
                    break;
                case InCondition inCondition:
                    VisitInCondition(ctx, inCondition);
                    break;
                case InQueryCondition inQuery:
                    VisitInQueryCondition(ctx, inQuery);
                    break;
                case NullCondition nullCondition:
                    VisitNullCondition(ctx, nullCondition);
                    break;
                case BooleanCondition booleanCondition:
                    VisitBooleanCondition(ctx, booleanCondition);
                    break;
                case NestedCondition nested:
                    VisitNestedCondition(ctx, nested);
                    break;
                case ExistsCondition exists:
                    VisitExistsCondition(ctx, exists);
                    break;
                case QueryCondition queryCondition:
                    VisitQueryCondition(ctx, queryCondition);
                    break;
                case SubQueryCondition subQuery:
                    VisitSubQueryCondition(ctx, subQuery);
                    break;
                case RawCondition raw:
                    VisitRawCondition(ctx, raw);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clause), $"Unsupported condition type: {clause.GetType().Name}");
            }
        }

        protected virtual void VisitRawCondition(CompileContext ctx, RawCondition x)
        {
            ctx.Bindings.AddRange(x.Bindings);
            ctx.Write(WrapIdentifiers(x.Expression));
        }

        protected virtual void VisitQueryCondition(CompileContext ctx, QueryCondition x)
        {
            ctx.Write(Wrap(x.Column)).Write(" ").Write(checkOperator(x.Operator)).Write(" (");
            CompileSubQuery(ctx, x.Query);
            ctx.Write(")");
        }

        protected virtual void VisitSubQueryCondition(CompileContext ctx, SubQueryCondition x)
        {
            ctx.Write("(");
            CompileSubQuery(ctx, x.Query);
            ctx.Write(") ").Write(checkOperator(x.Operator)).Write(" ").Write(Parameter(ctx, x.Value));
        }

        protected virtual void VisitBasicCondition(CompileContext ctx, BasicCondition x)
        {
            if (x.IsNot)
            {
                ctx.Write("NOT (");
            }

            ctx.Write(Wrap(x.Column)).Write(" ").Write(checkOperator(x.Operator)).Write(" ").Write(Parameter(ctx, x.Value));

            if (x.IsNot)
            {
                ctx.Write(")");
            }
        }

        protected virtual void VisitBasicStringCondition(CompileContext ctx, BasicStringCondition x)
        {
            var column = Wrap(x.Column);

            var value = Resolve(ctx, x.Value) as string;

            if (value == null)
            {
                throw new ArgumentException("Expecting a non nullable string");
            }

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

            if (!x.CaseSensitive)
            {
                column = CompileLower(column);
                value = value.ToLowerInvariant();
            }

            if (x.IsNot)
            {
                ctx.Write("NOT (");
            }

            if (x.Value is UnsafeLiteral)
            {
                ctx.Write(column).Write(" ").Write(checkOperator(method)).Write(" ").Write(value);
            }
            else
            {
                ctx.Write(column).Write(" ").Write(checkOperator(method)).Write(" ").Write(Parameter(ctx, value));
            }

            if (!string.IsNullOrEmpty(x.EscapeCharacter))
            {
                ctx.Write(" ESCAPE '").Write(x.EscapeCharacter).Write("'");
            }

            if (x.IsNot)
            {
                ctx.Write(")");
            }
        }

        protected virtual void VisitBasicDateCondition(CompileContext ctx, BasicDateCondition x)
        {
            var column = Wrap(x.Column);
            var op = checkOperator(x.Operator);

            if (x.IsNot)
            {
                ctx.Write("NOT (");
            }

            ctx.Write(x.Part.ToUpperInvariant()).Write("(").Write(column).Write(") ").Write(op).Write(" ").Write(Parameter(ctx, x.Value));

            if (x.IsNot)
            {
                ctx.Write(")");
            }
        }

        protected virtual void VisitNestedCondition(CompileContext ctx, NestedCondition x)
        {
            List<AbstractCondition> clauses;

            if (x.Conditions is ConditionGroup group)
            {
                if (!group.HasConditions(EngineCode))
                {
                    return;
                }

                clauses = group.GetConditions(EngineCode);
            }
            else
            {
                if (!(x.Conditions.HasComponent("where", EngineCode) || x.Conditions.HasComponent("having", EngineCode)))
                {
                    return;
                }

                var clause = x.Conditions.HasComponent("where", EngineCode) ? "where" : "having";
                clauses = x.Conditions.GetComponents<AbstractCondition>(clause, EngineCode);
            }

            var startLen = ctx.Buffer.Length;

            if (x.IsNot)
            {
                ctx.Write("NOT ");
            }

            ctx.Write("(");
            var innerStart = ctx.Buffer.Length;

            VisitConditions(ctx, clauses);

            if (ctx.Buffer.Length == innerStart)
            {
                // Nothing was written inside the nested group, roll back
                ctx.Buffer.Length = startLen;
                return;
            }

            ctx.Write(")");
        }

        protected virtual void VisitTwoColumnsCondition(CompileContext ctx, TwoColumnsCondition clause)
        {
            if (clause.IsNot)
            {
                ctx.Write("NOT ");
            }

            ctx.Write(Wrap(clause.First)).Write(" ").Write(checkOperator(clause.Operator)).Write(" ").Write(Wrap(clause.Second));
        }

        protected virtual void VisitBetweenCondition(CompileContext ctx, BetweenCondition item)
        {
            var between = item.IsNot ? "NOT BETWEEN" : "BETWEEN";
            ctx.Write(Wrap(item.Column)).Write(" ").Write(between).Write(" ")
               .Write(Parameter(ctx, item.Lower)).Write(" AND ").Write(Parameter(ctx, item.Higher));
        }

        protected virtual void VisitInCondition(CompileContext ctx, InCondition item)
        {
            ctx.Write(Wrap(item.Column));

            if (!item.Values.Any())
            {
                ctx.Write(item.IsNot ? " 1 = 1 /* NOT IN [empty list] */" : " 1 = 0 /* IN [empty list] */");
                return;
            }

            var inOperator = item.IsNot ? " NOT IN (" : " IN (";
            ctx.Write(inOperator).Write(Parameterize(ctx, item.Values)).Write(")");
        }

        protected virtual void VisitInQueryCondition(CompileContext ctx, InQueryCondition item)
        {
            var inOperator = item.IsNot ? " NOT IN (" : " IN (";
            ctx.Write(Wrap(item.Column)).Write(inOperator);
            CompileSubQuery(ctx, item.Query);
            ctx.Write(")");
        }

        protected virtual void VisitNullCondition(CompileContext ctx, NullCondition item)
        {
            var op = item.IsNot ? "IS NOT NULL" : "IS NULL";
            ctx.Write(Wrap(item.Column)).Write(" ").Write(op);
        }

        protected virtual void VisitBooleanCondition(CompileContext ctx, BooleanCondition item)
        {
            var value = item.Value ? CompileTrue() : CompileFalse();
            var op = item.IsNot ? "!=" : "=";
            ctx.Write(Wrap(item.Column)).Write(" ").Write(op).Write(" ").Write(value);
        }

        protected virtual void VisitExistsCondition(CompileContext ctx, ExistsCondition item)
        {
            var op = item.IsNot ? "NOT EXISTS (" : "EXISTS (";

            var query = item.Query.Clone();
            if (OmitSelectInsideExists)
            {
                query.SelectClauses.Clear();
                query.SelectRaw("1");
            }

            ctx.Write(op);
            CompileSubQuery(ctx, query);
            ctx.Write(")");
        }

        // Backward compatibility wrappers delegating to visitor methods
        protected virtual string CompileCondition(CompileContext ctx, AbstractCondition clause)
        {
            var temp = new CompileContext(ctx.CurrentQuery, parameterPlaceholder, EscapeCharacter);
            VisitCondition(temp, clause);
            ctx.Bindings.AddRange(temp.Bindings);
            return temp.Buffer.ToString();
        }

        protected virtual string CompileConditions(CompileContext ctx, List<AbstractCondition> conditions)
        {
            var temp = new CompileContext(ctx.CurrentQuery, parameterPlaceholder, EscapeCharacter);
            VisitConditions(temp, conditions);
            ctx.Bindings.AddRange(temp.Bindings);
            return temp.Buffer.ToString();
        }

        protected virtual string CompileRawCondition(CompileContext ctx, RawCondition x) => RunToString(ctx, c => VisitRawCondition(c, x));
        protected virtual string CompileQueryCondition(CompileContext ctx, QueryCondition x) => RunToString(ctx, c => VisitQueryCondition(c, x));
        protected virtual string CompileSubQueryCondition(CompileContext ctx, SubQueryCondition x) => RunToString(ctx, c => VisitSubQueryCondition(c, x));
        protected virtual string CompileBasicCondition(CompileContext ctx, BasicCondition x) => RunToString(ctx, c => VisitBasicCondition(c, x));
        protected virtual string CompileBasicStringCondition(CompileContext ctx, BasicStringCondition x) => RunToString(ctx, c => VisitBasicStringCondition(c, x));
        protected virtual string CompileBasicDateCondition(CompileContext ctx, BasicDateCondition x) => RunToString(ctx, c => VisitBasicDateCondition(c, x));
        protected virtual string CompileNestedCondition(CompileContext ctx, NestedCondition x) => RunToString(ctx, c => VisitNestedCondition(c, x));
        protected virtual string CompileTwoColumnsCondition(CompileContext ctx, TwoColumnsCondition clause) => RunToString(ctx, c => VisitTwoColumnsCondition(c, clause));
        protected virtual string CompileBetweenCondition(CompileContext ctx, BetweenCondition item) => RunToString(ctx, c => VisitBetweenCondition(c, item));
        protected virtual string CompileInCondition(CompileContext ctx, InCondition item) => RunToString(ctx, c => VisitInCondition(c, item));
        protected virtual string CompileInQueryCondition(CompileContext ctx, InQueryCondition item) => RunToString(ctx, c => VisitInQueryCondition(c, item));
        protected virtual string CompileNullCondition(CompileContext ctx, NullCondition item) => RunToString(ctx, c => VisitNullCondition(c, item));
        protected virtual string CompileBooleanCondition(CompileContext ctx, BooleanCondition item) => RunToString(ctx, c => VisitBooleanCondition(c, item));
        protected virtual string CompileExistsCondition(CompileContext ctx, ExistsCondition item) => RunToString(ctx, c => VisitExistsCondition(c, item));

        private string RunToString(CompileContext ctx, Action<CompileContext> action)
        {
            var temp = new CompileContext(ctx.CurrentQuery, parameterPlaceholder, EscapeCharacter);
            action(temp);
            ctx.Bindings.AddRange(temp.Bindings);
            return temp.Buffer.ToString();
        }
    }
}
