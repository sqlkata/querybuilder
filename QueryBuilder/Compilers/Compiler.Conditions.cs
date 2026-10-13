using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata.Compilers
{
    public partial class Compiler
    {
        protected virtual string CompileCondition(SqlResult ctx, AbstractCondition clause)
        {
            return clause switch
            {
                BasicDateCondition date => CompileBasicDateCondition(ctx, date),
                BasicStringCondition str => CompileBasicStringCondition(ctx, str),
                BasicCondition basic => CompileBasicCondition(ctx, basic),
                TwoColumnsCondition two => CompileTwoColumnsCondition(ctx, two),
                BetweenCondition between => CompileBetweenCondition(ctx, between),
                InCondition inCondition => CompileInCondition(ctx, inCondition),
                InQueryCondition inQuery => CompileInQueryCondition(ctx, inQuery),
                NullCondition nullCondition => CompileNullCondition(ctx, nullCondition),
                BooleanCondition booleanCondition => CompileBooleanCondition(ctx, booleanCondition),
                NestedCondition nested => CompileNestedCondition(ctx, nested),
                ExistsCondition exists => CompileExistsCondition(ctx, exists),
                QueryCondition queryCondition => CompileQueryCondition(ctx, queryCondition),
                SubQueryCondition subQuery => CompileSubQueryCondition(ctx, subQuery),
                RawCondition raw => CompileRawCondition(ctx, raw),
                _ => throw new ArgumentOutOfRangeException(nameof(clause), $"Unsupported condition type: {clause.GetType().Name}")
            };
        }

        protected virtual string CompileConditions(SqlResult ctx, List<AbstractCondition> conditions)
        {
            var result = new List<string>();

            for (var i = 0; i < conditions.Count; i++)
            {
                var compiled = CompileCondition(ctx, conditions[i]);

                if (string.IsNullOrEmpty(compiled))
                {
                    continue;
                }

                var boolOperator = i == 0 ? "" : (conditions[i].IsOr ? "OR " : "AND ");

                result.Add(boolOperator + compiled);
            }

            return string.Join(" ", result);
        }

        protected virtual string CompileRawCondition(SqlResult ctx, RawCondition x)
        {
            ctx.Bindings.AddRange(x.Bindings);
            return WrapIdentifiers(x.Expression);
        }

        protected virtual string CompileQueryCondition(SqlResult ctx, QueryCondition x)
        {
            var subCtx = CompileSelectQuery(x.Query);

            ctx.Bindings.AddRange(subCtx.Bindings);

            return Wrap(x.Column) + " " + checkOperator(x.Operator) + " (" + subCtx.RawSql + ")";
        }

        protected virtual string CompileSubQueryCondition(SqlResult ctx, SubQueryCondition x)
        {
            var subCtx = CompileSelectQuery(x.Query);

            ctx.Bindings.AddRange(subCtx.Bindings);

            return "(" + subCtx.RawSql + ") " + checkOperator(x.Operator) + " " + Parameter(ctx, x.Value);
        }

        protected virtual string CompileBasicCondition(SqlResult ctx, BasicCondition x)
        {
            var sql = $"{Wrap(x.Column)} {checkOperator(x.Operator)} {Parameter(ctx, x.Value)}";

            if (x.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        protected virtual string CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x)
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

            string sql;


            if (!x.CaseSensitive)
            {
                column = CompileLower(column);
                value = value.ToLowerInvariant();
            }

            if (x.Value is UnsafeLiteral)
            {
                sql = $"{column} {checkOperator(method)} {value}";
            }
            else
            {
                sql = $"{column} {checkOperator(method)} {Parameter(ctx, value)}";
            }

            if (!string.IsNullOrEmpty(x.EscapeCharacter))
            {
                sql = $"{sql} ESCAPE '{x.EscapeCharacter}'";
            }

            return x.IsNot ? $"NOT ({sql})" : sql;

        }

        protected virtual string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition x)
        {
            var column = Wrap(x.Column);
            var op = checkOperator(x.Operator);

            var sql = $"{x.Part.ToUpperInvariant()}({column}) {op} {Parameter(ctx, x.Value)}";

            return x.IsNot ? $"NOT ({sql})" : sql;
        }

        protected virtual string CompileNestedCondition(SqlResult ctx, NestedCondition x)
        {
            if (x.Conditions is ConditionGroup group)
            {
                if (!group.HasConditions(EngineCode))
                {
                    return null;
                }

                var sql = CompileConditions(ctx, group.GetConditions(EngineCode));
                return x.IsNot ? $"NOT ({sql})" : $"({sql})";
            }

            if (!(x.Conditions.HasComponent("where", EngineCode) || x.Conditions.HasComponent("having", EngineCode)))
            {
                return null;
            }

            var clause = x.Conditions.HasComponent("where", EngineCode) ? "where" : "having";

            var clauses = x.Conditions.GetComponents<AbstractCondition>(clause, EngineCode);

            var sql2 = CompileConditions(ctx, clauses);

            return x.IsNot ? $"NOT ({sql2})" : $"({sql2})";
        }

        protected string CompileTwoColumnsCondition(SqlResult ctx, TwoColumnsCondition clause)
        {
            var op = clause.IsNot ? "NOT " : "";
            return $"{op}{Wrap(clause.First)} {checkOperator(clause.Operator)} {Wrap(clause.Second)}";
        }

        protected virtual string CompileBetweenCondition(SqlResult ctx, BetweenCondition item)
        {
            var between = item.IsNot ? "NOT BETWEEN" : "BETWEEN";
            var lower = Parameter(ctx, item.Lower);
            var higher = Parameter(ctx, item.Higher);

            return Wrap(item.Column) + $" {between} {lower} AND {higher}";
        }

        protected virtual string CompileInCondition(SqlResult ctx, InCondition item)
        {
            var column = Wrap(item.Column);

            if (!item.Values.Any())
            {
                return item.IsNot ? $"1 = 1 /* NOT IN [empty list] */" : "1 = 0 /* IN [empty list] */";
            }

            var inOperator = item.IsNot ? "NOT IN" : "IN";

            var parameterizedValues = Parameterize(ctx, item.Values);

            return column + $" {inOperator} ({parameterizedValues})";
        }

        protected virtual string CompileInQueryCondition(SqlResult ctx, InQueryCondition item)
        {

            var subCtx = CompileSelectQuery(item.Query);

            ctx.Bindings.AddRange(subCtx.Bindings);

            var inOperator = item.IsNot ? "NOT IN" : "IN";

            return Wrap(item.Column) + $" {inOperator} ({subCtx.RawSql})";
        }

        protected virtual string CompileNullCondition(SqlResult ctx, NullCondition item)
        {
            var op = item.IsNot ? "IS NOT NULL" : "IS NULL";
            return Wrap(item.Column) + " " + op;
        }

        protected virtual string CompileBooleanCondition(SqlResult ctx, BooleanCondition item)
        {
            var column = Wrap(item.Column);
            var value = item.Value ? CompileTrue() : CompileFalse();

            var op = item.IsNot ? "!=" : "=";

            return $"{column} {op} {value}";
        }

        protected virtual string CompileExistsCondition(SqlResult ctx, ExistsCondition item)
        {
            var op = item.IsNot ? "NOT EXISTS" : "EXISTS";


            // remove unneeded components
            var query = item.Query.Clone();

            if (OmitSelectInsideExists)
            {
                query.ClearComponent("select").SelectRaw("1");
            }

            var subCtx = CompileSelectQuery(query);

            ctx.Bindings.AddRange(subCtx.Bindings);

            return $"{op} ({subCtx.RawSql})";
        }
    }
}
