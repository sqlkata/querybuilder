using System;
using System.Linq;
using SqlKata.Interfaces;

namespace SqlKata
{
    public partial class Query
    {

        public IQuery Combine(string operation, bool all, IQuery query)
        {
            if (this.Method != "select" || query.Method != "select")
            {
                throw new InvalidOperationException("Only select queries can be combined.");
            }

            return AddComponent("combine", new Combine
            {
                Query = query,
                Operation = operation,
                All = all,
            });
        }

        public IQuery CombineRaw(string sql, params object[] bindings)
        {
            if (this.Method != "select")
            {
                throw new InvalidOperationException("Only select queries can be combined.");
            }

            return AddComponent("combine", new RawCombine
            {
                Expression = sql,
                Bindings = Helper.Flatten(bindings).ToArray(),
            });
        }

        public IQuery Union(IQuery query, bool all = false)
        {
            return Combine("union", all, query);
        }

        public IQuery UnionAll(IQuery query)
        {
            return Union(query, true);
        }

        public IQuery Union(Func<IQuery, IQuery> callback, bool all = false)
        {
            var query = callback.Invoke(new Query());
            return Union(query, all);
        }

        public IQuery UnionAll(Func<IQuery, IQuery> callback)
        {
            return Union(callback, true);
        }

        public IQuery UnionRaw(string sql, params object[] bindings) => CombineRaw(sql, bindings);

        public IQuery Except(IQuery query, bool all = false)
        {
            return Combine("except", all, query);
        }

        public IQuery ExceptAll(IQuery query)
        {
            return Except(query, true);
        }

        public IQuery Except(Func<IQuery, IQuery> callback, bool all = false)
        {
            var query = callback.Invoke(new Query());
            return Except(query, all);
        }

        public IQuery ExceptAll(Func<IQuery, IQuery> callback)
        {
            return Except(callback, true);
        }
        public IQuery ExceptRaw(string sql, params object[] bindings) => CombineRaw(sql, bindings);

        public IQuery Intersect(IQuery query, bool all = false)
        {
            return Combine("intersect", all, query);
        }

        public IQuery IntersectAll(IQuery query)
        {
            return Intersect(query, true);
        }

        public IQuery Intersect(Func<IQuery, IQuery> callback, bool all = false)
        {
            var query = callback.Invoke(new Query());
            return Intersect(query, all);
        }

        public IQuery IntersectAll(Func<IQuery, IQuery> callback)
        {
            return Intersect(callback, true);
        }
        public IQuery IntersectRaw(string sql, params object[] bindings) => CombineRaw(sql, bindings);

    }
}