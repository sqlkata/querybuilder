namespace SqlKata
{
    public partial class Query
    {
        private Query Join(Func<Join, Join> callback)
        {
            var join = callback.Invoke(new Join(new Query()).AsInner());

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
            return Join(j =>
            {
                var join = new Join(j.JoinWith(table).BaseQuery);
                join.BaseQuery.WhereColumns(first, op, second);
                return join.AsType(type);
            });
        }

        public Query Join(string table, Func<Join, Join> callback, string type = "inner join")
        {
            return Join(j =>
            {
                var join = new Join(j.JoinWith(table).BaseQuery);
                join.BaseQuery.Where(q => callback(new Join(q)).BaseQuery);
                return join.AsType(type);
            });
        }

        public Query Join(Query query, Func<Join, Join> onCallback, string type = "inner join")
        {
            return Join(j =>
            {
                var join = new Join(j.JoinWith(query).BaseQuery);
                join.BaseQuery.Where(q => onCallback(new Join(q)).BaseQuery);
                return join.AsType(type);
            });
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
    }
}
