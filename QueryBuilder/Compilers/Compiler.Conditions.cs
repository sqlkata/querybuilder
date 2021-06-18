using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata.Compilers
{
    public partial class Compiler
    {
        protected virtual MethodInfo FindCompilerMethodInfo(Type clauseType, string methodName)
        {
            return _compileConditionMethodsProvider.GetMethodInfo(clauseType, methodName);
        }

        protected virtual string CompileCondition(SqlResult ctx, AbstractCondition clause)
        {
            var clauseType = clause.GetType();

            var name = clauseType.Name;

            name = name.Substring(0, name.IndexOf("Condition"));

            var methodName = "Compile" + name + "Condition";

            var methodInfo = FindCompilerMethodInfo(clauseType, methodName);

            try
            {

                var result = methodInfo.Invoke(this, new object[] {
                    ctx,
                    clause
                });

                return result as string;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to invoke '{methodName}'", ex);
            }

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

        protected virtual string CompileQueryCondition<T>(SqlResult ctx, QueryCondition<T> x) where T : BaseQuery<T>
        {
            var subCtx = CompileSelectQuery(x.Query);

            ctx.Bindings.AddRange(subCtx.Bindings);

            return Wrap(x.Column) + " " + checkOperator(x.Operator) + " (" + subCtx.RawSql + ")";
        }

        protected virtual string CompileSubQueryCondition<T>(SqlResult ctx, SubQueryCondition<T> x) where T : BaseQuery<T>
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

        protected virtual string CompileNestedCondition<Q>(SqlResult ctx, NestedCondition<Q> x) where Q : BaseQuery<Q>
        {
            if (!(x.Query.HasComponent("where", EngineCode) || x.Query.HasComponent("having", EngineCode)))
            {
                return null;
            }

            var clause = x.Query.HasComponent("where", EngineCode) ? "where" : "having";

            var clauses = x.Query.GetComponents<AbstractCondition>(clause, EngineCode);

            var sql = CompileConditions(ctx, clauses);

            return x.IsNot ? $"NOT ({sql})" : $"({sql})";
        }

        protected string CompileTwoColumnsCondition(SqlResult ctx, TwoColumnsCondition clause)
        {
            var op = clause.IsNot ? "NOT " : "";
            return $"{op}{Wrap(clause.First)} {checkOperator(clause.Operator)} {Wrap(clause.Second)}";
        }

        protected virtual string CompileBetweenCondition<T>(SqlResult ctx, BetweenCondition<T> item)
        {
            var between = item.IsNot ? "NOT BETWEEN" : "BETWEEN";
            var lower = Parameter(ctx, item.Lower);
            var higher = Parameter(ctx, item.Higher);

            return Wrap(item.Column) + $" {between} {lower} AND {higher}";
        }

        protected virtual string CompileInCondition<T>(SqlResult ctx, InCondition<T> item)
        {
            var column = Wrap(item.Column);

            if (!item.Values.Any())
            {
                return item.IsNot ? $"1 = 1 /* NOT IN [empty list] */" : "1 = 0 /* IN [empty list] */";
            }

            var inOperator = item.IsNot ? "NOT IN" : "IN";

            var values = Parameterize(ctx, item.Values);

            return column + $" {inOperator} ({values})";
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

            var subCtx = CompileSelectQuery(item.Query);

            ctx.Bindings.AddRange(subCtx.Bindings);

            return $"{op} ({subCtx.RawSql})";
        }
    }
}
