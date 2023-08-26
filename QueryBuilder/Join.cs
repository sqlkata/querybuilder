using System;

namespace SqlKata
{
    public class Join : BaseQuery<Join>
    {
        protected string TypeField = "inner join";

        public string Type
        {
            get => TypeField;
            set => TypeField = value.ToUpperInvariant();
        }

        public override Join Clone()
        {
            var clone = base.Clone();
            clone.TypeField = TypeField;
            return clone;
        }

        public Join AsType(string type)
        {
            Type = type;
            return this;
        }

        /// <summary>
        ///     Alias for "from" operator.
        ///     Since "from" does not sound well with join clauses
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public Join JoinWith(string table)
        {
            return From(table);
        }

        public Join JoinWith(Query query)
        {
            return From(query);
        }

        public Join JoinWith(Func<Query, Query> callback)
        {
            return From(callback);
        }

        public Join AsInner()
        {
            return AsType("inner join");
        }

        public Join AsOuter()
        {
            return AsType("outer join");
        }

        public Join AsLeft()
        {
            return AsType("left join");
        }

        public Join AsRight()
        {
            return AsType("right join");
        }

        public Join AsCross()
        {
            return AsType("cross join");
        }

        public Join On(string first, string second, string op = "=")
        {
            return AddComponent("where", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Join OrOn(string first, string second, string op = "=")
        {
            return Or().On(first, second, op);
        }

        public override Join NewQuery()
        {
            return new Join();
        }
    }
}
