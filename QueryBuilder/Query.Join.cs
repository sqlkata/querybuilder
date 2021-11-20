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
            string type = "inner join"
        )
        {
            return Join(j => j.JoinWith(table).WhereColumns(first, op, second).AsType(type));
        }

        public Query Join(string table, Func<Join, Join> callback, string type = "inner join")
        {
            return Join(j => j.JoinWith(table).Where(callback).AsType(type));
        }

        public Query Join(Query query, Func<Join, Join> onCallback, string type = "inner join")
        {
            return Join(j => j.JoinWith(query).Where(onCallback).AsType(type));
        }

        public Query LeftJoin(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "left join");
        }

        public Query LeftJoin(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "left join");
        }

        public Query LeftJoin(Query query, Func<Join, Join> onCallback)
        {
            return Join(query, onCallback, "left join");
        }

        public Query RightJoin(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "right join");
        }

        public Query RightJoin(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "right join");
        }

        public Query RightJoin(Query query, Func<Join, Join> onCallback)
        {
            return Join(query, onCallback, "right join");
        }

        public Query CrossJoin(string table)
        {
            return Join(j => j.JoinWith(table).AsCross());
        }

        public Query CrossApply(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "cross apply");
        }

        public Query CrossApply(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "cross apply");
        }

        public Query CrossApply(Query query, Func<Join, Join> onCallback)
        {
            return Join(query, onCallback, "cross apply");
        }

        public Query CrossApply(Query query)
        {
            return Join(query, j => j, "cross apply");
        }

        public Query OuterApply(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "outer apply");
        }

        public Query OuterApply(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "outer apply");
        }

        public Query OuterApply(Query query, Func<Join, Join> onCallback)
        {
            return Join(query, onCallback, "outer apply");
        }

        public Query OuterApply(Query query)
        {
            return Join(query, j => j, "outer apply");
        }

    }
}
