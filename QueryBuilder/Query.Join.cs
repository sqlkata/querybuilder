using System;
using SqlKata.Interfaces;

namespace SqlKata
{
    public partial class Query
    {

        private IQuery Join(Func<Join, Join> callback)
        {
            var join = callback.Invoke(new Join().AsInner());

            return AddComponent("join", new BaseJoin
            {
                Join = join
            });
        }

        public IQuery Join(
            string table,
            string first,
            string second,
            string op = "=",
            string type = "inner join"
        )
        {
            return Join(j => j.JoinWith(table).WhereColumns(first, op, second).AsType(type));
        }

        public IQuery Join(string table, Func<Join, Join> callback, string type = "inner join")
        {
            return Join(j => j.JoinWith(table).Where(callback).AsType(type));
        }

        public IQuery Join(IQuery query, Func<Join, Join> onCallback, string type = "inner join")
        {
            return Join(j => j.JoinWith(query).Where(onCallback).AsType(type));
        }

        public IQuery LeftJoin(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "left join");
        }

        public IQuery LeftJoin(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "left join");
        }

        public IQuery LeftJoin(IQuery query, Func<Join, Join> onCallback)
        {
            return Join(query, onCallback, "left join");
        }

        public IQuery RightJoin(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "right join");
        }

        public IQuery RightJoin(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "right join");
        }

        public IQuery RightJoin(IQuery query, Func<Join, Join> onCallback)
        {
            return Join(query, onCallback, "right join");
        }

        public IQuery CrossJoin(string table)
        {
            return Join(j => j.JoinWith(table).AsCross());
        }

    }
}