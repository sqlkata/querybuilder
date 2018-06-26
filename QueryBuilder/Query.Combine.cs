using System;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        #region Combine
        /// <summary>
        /// Allows you to combine multiple queries using one of the following available operators 
        /// <c>union, intersect and except</c> by providing the following methods Union, UnionAll, Intersect, IntersectAll, 
        /// Except and ExceptAll. The method accepts either an instance of Query or a labmda expression
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="all"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public Query Combine(string operation, bool all, Query query)
        {
            if (Method != "select" || query.Method != "select")
                throw new InvalidOperationException("Only select queries can be combined.");

            return AddComponent("combine", new Combine
            {
                Query = query,
                Operation = operation,
                All = all
            });
        }

        /// <summary>
        /// Allows you to combine multiple queries using a RAW combine statement
        /// </summary>
        /// <param name="sql">The sql</param>
        /// <param name="bindings">The bindings</param>
        /// <returns></returns>
        public Query CombineRaw(string sql, params object[] bindings)
        {
            if (Method != "select")
                throw new InvalidOperationException("Only select queries can be combined.");

            return AddComponent("combine", new RawCombine
            {
                Expression = sql,
                Bindings = Helper.Flatten(bindings).ToArray()
            });
        }
        #endregion

        #region Union
        public Query Union(Query query, bool all = false)
        {
            return Combine("union", all, query);
        }

        public Query UnionAll(Query query)
        {
            return Union(query, true);
        }

        public Query Union(Func<Query, Query> callback, bool all = false)
        {
            var query = callback.Invoke(new Query());
            return Union(query, all);
        }

        public Query UnionAll(Func<Query, Query> callback)
        {
            return Union(callback, true);
        }

        public Query UnionRaw(string sql, params object[] bindings) => CombineRaw(sql, bindings);

        public Query Except(Query query, bool all = false)
        {
            return Combine("except", all, query);
        }
        #endregion

        #region Except
        public Query ExceptAll(Query query)
        {
            return Except(query, true);
        }

        public Query Except(Func<Query, Query> callback, bool all = false)
        {
            var query = callback.Invoke(new Query());
            return Except(query, all);
        }

        public Query ExceptAll(Func<Query, Query> callback)
        {
            return Except(callback, true);
        }

        public Query ExceptRaw(string sql, params object[] bindings) => CombineRaw(sql, bindings);

        public Query Intersect(Query query, bool all = false)
        {
            return Combine("intersect", all, query);
        }
        #endregion

        #region Intersec
        public Query IntersectAll(Query query)
        {
            return Intersect(query, true);
        }

        public Query Intersect(Func<Query, Query> callback, bool all = false)
        {
            var query = callback.Invoke(new Query());
            return Intersect(query, all);
        }

        public Query IntersectAll(Func<Query, Query> callback)
        {
            return Intersect(callback, true);
        }

        public Query IntersectRaw(string sql, params object[] bindings) => CombineRaw(sql, bindings);
        #endregion
    }
}