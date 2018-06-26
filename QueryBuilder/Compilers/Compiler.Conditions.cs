using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata.Compilers
{
    public partial class Compiler
    {

        protected virtual string CompileCondition(AbstractCondition clause)
        {
            var name = clause.GetType().Name;
            name = name.Substring(0, name.IndexOf("Condition"));

            var methodName = "Compile" + name + "Condition";

            var clauseType = clause.GetType();
            MethodInfo methodInfo = this.GetType().GetRuntimeMethods().Where(x => x.Name == methodName).FirstOrDefault();

            if (methodInfo == null)
            {
                throw new Exception($"Failed to locate a compiler for {name}.");
            }

            if (clauseType.IsConstructedGenericType && methodInfo.GetGenericArguments().Any())
            {
                methodInfo = methodInfo.MakeGenericMethod(clauseType.GenericTypeArguments);
            }

            var result = methodInfo.Invoke(this, new object[] { clause });

            return result as string;
        }

        protected virtual string CompileConditions(List<AbstractCondition> conditions)
        {
            var sql = conditions
                .Select(x => CompileCondition(x))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList()
                .Select((x, i) =>
                {
                    var boolOperator = i == 0 ? "" : (conditions[i].IsOr ? "OR " : "AND ");
                    return boolOperator + x;
                }).ToList();

            return JoinComponents(sql, "conditions");
        }

        protected virtual string CompileRawCondition(RawCondition x)
        {
            bindings.AddRange(x.Bindings);
            return WrapIdentifiers(x.Expression);
        }

        protected virtual string CompileQueryCondition<T>(QueryCondition<T> x) where T : BaseQuery<T>
        {
            var select = CompileQuery(x.Query);

            return Wrap(x.Column) + " " + x.Operator + " (" + select + ")";
        }

        protected virtual string CompileBasicCondition<T>(BasicCondition<T> x)
        {
            var sql = Wrap(x.Column) + " " + x.Operator + " " + Parameter(x.Value);

            if (x.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        protected virtual string CompileBasicStringCondition(BasicStringCondition x)
        {
            var column = Wrap(x.Column);

            if (!x.CaseSensitive)
            {
                x.Value = x.Value.ToLower();
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

            var sql = column + " " + method + " " + Parameter(x.Value);

            if (x.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        protected virtual string CompileBasicDateCondition(BasicDateCondition x)
        {
            var column = Wrap(x.Column);

            var sql = $"{x.Part.ToUpper()}({column}) {x.Operator} {Parameter(x.Value)}";

            return x.IsNot
                ? $"NOT ({sql})"
                : sql;
        }

        protected virtual string CompileNestedCondition<Q>(NestedCondition<Q> x) where Q : BaseQuery<Q>
        {
            if (!x.Query.HasComponent("where", EngineCode))
            {
                return null;
            }

            var sql = CompileConditions(x.Query.GetComponents<AbstractCondition>("where", EngineCode));
            var op = x.IsNot ? "NOT " : "";

            return string.IsNullOrEmpty(sql)
                ? ""
                : $"{op}({sql})";
        }

        protected string CompileTwoColumnsCondition(TwoColumnsCondition clause)
        {
            var op = clause.IsNot ? "NOT " : "";
            return $"{op}{Wrap(clause.First)} {clause.Operator} {Wrap(clause.Second)}";
        }

        protected virtual string CompileBetweenCondition<T>(BetweenCondition<T> item)
        {
            bindings.AddRange(new object[] { item.Lower, item.Higher });

            var between = item.IsNot ? "NOT BETWEEN" : "BETWEEN";

            return Wrap(item.Column) + $" {between} ? AND ?";
        }

        protected virtual string CompileInCondition<T>(InCondition<T> item)
        {
            if (!item.Values.Any())
            {
                return item.IsNot ? "1 = 1" : "1 = 0";
            }

            var inOperator = item.IsNot ? "NOT IN" : "IN";

            var values = Parameterize(item.Values);

            return Wrap(item.Column) + $" {inOperator} ({values})";
        }

        protected virtual string CompileInQueryCondition(InQueryCondition item)
        {

            var compiled = CompileQuery(item.Query);

            var inOperator = item.IsNot ? "NOT IN" : "IN";

            return Wrap(item.Column) + $" {inOperator} ({compiled})";
        }

        protected virtual string CompileNullCondition(NullCondition item)
        {
            var op = item.IsNot ? "IS NOT NULL" : "IS NULL";
            return Wrap(item.Column) + " " + op;
        }

        protected virtual string CompileExistsCondition<T>(ExistsCondition<T> item) where T : BaseQuery<T>
        {
            var op = item.IsNot ? "NOT EXISTS" : "EXISTS";
            return op + " (" + CompileQuery(item.Query) + ")";
        }

    }
}