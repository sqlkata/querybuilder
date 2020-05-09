using System;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {

        public Query Select(params string[] columns)
        {
            Method = QueryMethod.Select;

            columns = columns
                .Select(Helper.ExpandExpression)
                .SelectMany(x => x)
                .ToArray();


            foreach (var column in columns)
            {
                AddComponent(ClauseComponent.Select, new Column
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
            Method = QueryMethod.Select;

            AddComponent(ClauseComponent.Select, new RawColumn
            {
                Expression = sql,
                Bindings = bindings,
            });

            return this;
        }

        public Query Select(Query query, string alias)
        {
            Method = QueryMethod.Select;

            query = query.Clone();

            AddComponent(ClauseComponent.Select, new QueryColumn
            {
                Query = query.As(alias),
            });

            return this;
        }

        public Query Select(Func<Query, Query> callback, string alias)
        {
            return Select(callback.Invoke(NewChild()), alias);
        }
    }
}