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
                var result = methodInfo.Invoke(this, new object[] { ctx, clause });
                return result as string;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to invoke '{methodName}'", ex);
            }

        }

        protected virtual string CompileConditions(SqlResult ctx, List<AbstractCondition> conditions)
        {
            var sql = conditions
                .Select(x => CompileCondition(ctx, x))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList()
                .Select((x, i) =>
                {
                    var boolOperator = i == 0 ? "" : (conditions[i].IsOr ? "OR " : "AND ");
                    return boolOperator + x;
                }).ToList();

            return string.Join(" ", sql);
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

        protected virtual string CompileBasicCondition(SqlResult ctx, BasicCondition x)
        {
            var sql = Wrap(x.Column) + " " + checkOperator(x.Operator) + " " + Parameter(ctx, x.Value);

            if (x.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        protected virtual string CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x)
        {
            var column = Wrap(x.Column);

            var value = x.Value as string;

            if (value == null)
            {
                throw new ArgumentException("The value should be a non null value of type string");
            }

            if (!x.CaseSensitive)
            {
                x.Value = value.ToLower();
                column = CompileLower(column);
            }

            var method = x.Operator;

            if (new[] { "starts", "ends", "contains", "like" }.Contains(x.Operator))
            {

                method = "LIKE";

                if (x.Operator == "starts")
                {
                    x.Value = x.Value + "%";
                }
                else if (x.Operator == "ends")
                {
                    x.Value = "%" + x.Value;
                }
                else if (x.Operator == "contains")
                {
                    x.Value = "%" + x.Value + "%";
                }
                else
                {
                    x.Value = x.Value;
                }
            }

            var sql = column + " " + checkOperator(method) + " " + Parameter(ctx, x.Value);

            if (x.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        protected virtual string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition x)
        {
            var column = Wrap(x.Column);

            var sql = $"{x.Part.ToUpper()}({column}) {checkOperator(x.Operator)} {Parameter(ctx, x.Value)}";

            return x.IsNot
                ? $"NOT ({sql})"
                : sql;
        }

        protected virtual string CompileNestedCondition<Q>(SqlResult ctx, NestedCondition<Q> x) where Q : BaseQuery<Q>
        {
            if (!x.Query.HasComponent("where", EngineCode))
            {
                return null;
            }

            var sql = CompileConditions(ctx, x.Query.GetComponents<AbstractCondition>("where", EngineCode));
            var op = x.IsNot ? "NOT " : "";

            return string.IsNullOrEmpty(sql)
                ? ""
                : $"{op}({sql})";
        }

        protected string CompileTwoColumnsCondition(SqlResult ctx, TwoColumnsCondition clause)
        {
            var op = clause.IsNot ? "NOT " : "";
            return $"{op}{Wrap(clause.First)} {checkOperator(clause.Operator)} {Wrap(clause.Second)}";
        }

        protected virtual string CompileBetweenCondition<T>(SqlResult ctx, BetweenCondition<T> item)
        {
            ctx.Bindings.AddRange(new object[] { item.Lower, item.Higher });

            var between = item.IsNot ? "NOT BETWEEN" : "BETWEEN";

            return Wrap(item.Column) + $" {between} ? AND ?";
        }

        protected virtual string CompileInCondition<T>(SqlResult ctx, InCondition<T> item)
        {
            var column = Wrap(item.Column);

            if (!item.Values.Any())
            {
                return item.IsNot ? $"1 = 1 /* WhereNotIn({column}, [empty list]) */" : "1 = 0 /* WhereIn({column}, [empty list]) */";
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
