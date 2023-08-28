using System.Collections.Immutable;

namespace SqlKata
{
    public partial class Query
    {
        public Query Combine(string operation, bool all, Query query)
        {
            if (Method != "select" || query.Method != "select")
                throw new InvalidOperationException("Only select queries can be combined.");

            return AddComponent(new Combine
            {
                Engine = EngineScope,
                Component = "combine",
                Query = query,
                Operation = operation,
                All = all
            });
        }

        public Query CombineRaw(string sql, params object[] bindings)
        {
            if (Method != "select") throw new InvalidOperationException("Only select queries can be combined.");

            return AddComponent(new RawCombine
            {
                Engine = EngineScope,  
                Component = "combine",
                Expression = sql,
                Bindings = bindings.ToImmutableArray()
            });
        }

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

        public Query UnionRaw(string sql, params object[] bindings)
        {
            return CombineRaw(sql, bindings);
        }

        public Query Except(Query query, bool all = false)
        {
            return Combine("except", all, query);
        }

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

        public Query ExceptRaw(string sql, params object[] bindings)
        {
            return CombineRaw(sql, bindings);
        }

        public Query Intersect(Query query, bool all = false)
        {
            return Combine("intersect", all, query);
        }

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

        public Query IntersectRaw(string sql, params object[] bindings)
        {
            return CombineRaw(sql, bindings);
        }
    }
}