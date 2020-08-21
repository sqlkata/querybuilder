using System;
using System.Linq;
using System.Linq.Expressions;
using SqlKata.SqlExpressions;

namespace SqlKata
{
    public partial class Query
    {

        public Query Select(params string[] columns)
        {
            Method = "select";

            columns = columns
                .Select(x => Helper.ExpandExpression(x))
                .SelectMany(x => x)
                .ToArray();


            foreach (var column in columns)
            {
                AddComponent("select", new Column
                {
                    Name = column
                });
            }

            return this;
        }

        /// <summary>
        /// Add a new "raw" select expression to the query.
        /// </summary>
        /// <returns></returns>
        public Query SelectRaw(string sql, params object[] bindings)
        {
            Method = "select";

            AddComponent("select", new RawColumn
            {
                Expression = sql,
                Bindings = bindings,
            });

            return this;
        }

        public Query Select(Query query, string alias)
        {
            Method = "select";

            query = query.Clone();

            AddComponent("select", new QueryColumn
            {
                Query = query.As(alias),
            });

            return this;
        }

        public Query Select(Func<Query, Query> callback, string alias)
        {
            return Select(callback.Invoke(NewChild()), alias);
        }

        public Query Select(AbstractSqlExpression expression, string alias = null)
        {
            AddComponent("select", new SelectSqlExpressionClause
            {
                Expression = expression,
                Alias = alias
            });

            return this;
        }

        public Query Select(Expression expression, string alias = null)
        {
            AddComponent("select", new SelectExpressionClause
            {
                Expression = expression,
                Alias = alias
            });

            return this;
        }
    }
}