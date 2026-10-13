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
            return AddOrReplace(FromClauses, new FromClause
            {
                Table = table,
            });
        }

        public Query From(Query query, string alias = null)
        {
            query = query.Clone();

            if (alias != null)
            {
                query.As(alias);
            }

            return AddOrReplace(FromClauses, new QueryFromClause
            {
                Query = query
            });
        }

        public Query FromRaw(string sql, params object[] bindings)
        {
            return AddOrReplace(FromClauses, new RawFromClause
            {
                Expression = sql,
                Bindings = bindings,
            });
        }

        public Query From(Func<Query, Query> callback, string alias = null)
        {
            var query = NewQuery();

            return From(callback.Invoke(query), alias);
        }
    }
}
