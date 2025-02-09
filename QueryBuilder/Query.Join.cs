using System;

namespace SqlKata
{
    public partial class Query
    {

        private Query Join(Func<Join, Join> callback)
        {
            var join = callback.Invoke(new Join().AsInner());

            return AddComponent("join", new BaseJoin
            {
                Join = join
            });
        }

        public Query Join(
            string table,
            string first,
            string second,
            string op = "=",
            string type = "inner join",
            string indexHint = null
        )
        {
            return Join(j => j.JoinWith(table).WhereColumns(first, op, second).AsType(type).UsingIndexHint(indexHint));
        }

        public Query Join(string table, Func<Join, Join> callback, string type = "inner join", string indexHint = null)
        {
            return Join(j => j.JoinWith(table).Where(callback).AsType(type).UsingIndexHint(indexHint));
        }

        public Query Join(Query query, Func<Join, Join> onCallback, string type = "inner join", string indexHint = null)
        {
            return Join(j => j.JoinWith(query).Where(onCallback).AsType(type).UsingIndexHint(indexHint));
        }

        public Query LeftJoin(string table, string first, string second, string op = "=", string indexHint = null)
        {
            return Join(table, first, second, op, "left join", indexHint);
        }

        public Query LeftJoin(string table, Func<Join, Join> callback, string indexHint = null)
        {
            return Join(table, callback, "left join", indexHint);
        }

        public Query LeftJoin(Query query, Func<Join, Join> onCallback, string indexHint = null)
        {
            return Join(query, onCallback, "left join", indexHint);
        }

        public Query RightJoin(string table, string first, string second, string op = "=", string indexHint = null)
        {
            return Join(table, first, second, op, "right join", indexHint);
        }

        public Query RightJoin(string table, Func<Join, Join> callback, string indexHint = null)
        {
            return Join(table, callback, "right join", indexHint);
        }

        public Query RightJoin(Query query, Func<Join, Join> onCallback, string indexHint = null)
        {
            return Join(query, onCallback, "right join", indexHint);
        }

        public Query CrossJoin(string table)
        {
            return Join(j => j.JoinWith(table).AsCross());
        }

    }
}
