using System;

namespace SqlKata
{
    public partial class Query
    {
        public Query Join(Func<Join, Join> callback)
        {
            var join = callback.Invoke(new Join().AsInner());

            return Add("join", new BaseJoin
            {
                Join = join
            });
        }

        public Query Join(
            string table,
            string first,
            string second,
            string op = "=",
            string type = "inner"
        )
        {
            return Join(j => j.JoinWith(table).WhereColumns(first, op, second).AsType(type));
        }

        public Query Join(string table, Func<Join, Join> callback, string type = "inner")
        {
            return Join(j => j.JoinWith(table).Where(callback).AsType(type));
        }

        public Query Join(Query query, string type = "inner")
        {
            return Join(j => j.JoinWith(query).AsType(type));
        }

        public Query LeftJoin(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "left");
        }

        public Query LeftJoin(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "left");
        }

        public Query RightJoin(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "right");
        }

        public Query RightJoin(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "right");
        }

        public Query CrossJoin(string table)
        {
            return Join(j => j.JoinWith(table).AsCross());
        }

    }
}