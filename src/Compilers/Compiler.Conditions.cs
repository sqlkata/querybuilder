using System.Collections.Generic;
using System.Linq;

namespace SqlKata.Compilers
{
    public partial class Compiler
    {

        protected virtual string CompileCondition(AbstractCondition clause)
        {
            var name = clause.GetType().Name;
            name = name.Substring(0, name.IndexOf("Condition"));

            var methodName = "Compile" + name + "Condition";
            return dynamicCompile(methodName, clause);
        }

        protected virtual string CompileConditions(List<AbstractCondition> conditions)
        {
            var sql = new List<string>();

            for (var i = 0; i < conditions.Count; i++)
            {
                var compiled = CompileCondition(conditions[i]);

                if (string.IsNullOrEmpty(compiled))
                {
                    continue;
                }

                var boolOperator = i == 0 ? "" : (conditions[i].IsOr ? "OR " : "AND ");

                sql.Add(boolOperator + compiled);
            }

            return JoinComponents(sql, "conditions");
        }

        protected virtual string CompileRawCondition(RawCondition x)
        {
            return x.Expression;
        }

        protected virtual string CompileSubQueryCondition<T>(QueryCondition<T> x) where T : BaseQuery<T>
        {
            var select = CompileQuery(x.Query);

            var alias = string.IsNullOrEmpty(x.Query._Alias) ? "" : " AS " + x.Query._Alias;

            return Wrap(x.Column) + " " + x.Operator + " (" + select + ")" + alias;

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

        protected virtual string CompileNestedCondition<Q>(NestedCondition<Q> x) where Q : BaseQuery<Q>
        {
            if (!x.Query.Has("where"))
            {
                return null;
            }

            var sql = CompileConditions(x.Query.Get<AbstractCondition>("where"));
            var op = x.IsNot ? "NOT " : "";

            return $"{op}({sql})";
        }

        protected string CompileTwoColumnsCondition(TwoColumnsCondition clause)
        {
            var op = clause.IsNot ? "NOT " : "";
            return $"{op}{Wrap(clause.First)} {clause.Operator} {Wrap(clause.Second)}";
        }

        protected virtual string CompileBetweenCondition<T>(BetweenCondition<T> item)
        {
            var between = item.IsNot ? "NOT BETWEEN" : "BETWEEN";
            return Wrap(item.Column) + $" {between} {Parameter(item.Lower)} AND {Parameter(item.Higher)}";
        }

        protected virtual string CompileInCondition<T>(InCondition<T> item)
        {
            if (!item.Values.Any())
            {
                return item.IsNot ? "1 = 1" : "1 = 0";
            }

            var inOperator = item.IsNot ? "NOT IN" : "IN";

            var values = Parametrize(item.Values);

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