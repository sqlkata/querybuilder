using System;

namespace SqlKata
{
    public partial class Query
    {
        /// <summary>
        /// Add a from Clause
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public Query From(string table)
        {
            return AddOrReplaceComponent("from", new FromClause
            {
                Table = table,
            });
        }

        public Query From(Query query, string alias = null)
        {
            query = query.Clone();
            query.SetParent(this);

            if (alias != null)
            {
                query.As(alias);
            }

            return AddOrReplaceComponent("from", new QueryFromClause
            {
                Query = query
            });
        }

        public Query FromRaw(string sql, params object[] bindings)
        {
            return AddOrReplaceComponent("from", new RawFromClause
            {
                Expression = sql,
                Bindings = bindings,
            });
        }

        public Query From(Func<Query, Query> callback, string alias = null)
        {
            var query = new Query();

            query.SetParent(this);

            return From(callback.Invoke(query), alias);
        }
    }
}
