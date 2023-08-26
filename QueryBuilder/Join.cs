using System;

namespace SqlKata
{
    public sealed class Join 
    {
        public Query BaseQuery { get; }

        public Join(Query baseQuery)
        {
            BaseQuery = baseQuery;
        }

        protected string TypeField = "inner join";

        public string Type
        {
            get => TypeField;
            set => TypeField = value.ToUpperInvariant();
        }

        public Join Clone()
        {
            return new Join(BaseQuery)
            {
                TypeField = TypeField
            };
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
            return new Join(BaseQuery.From(table));
        }

        public Join JoinWith(Query query)
        {
            return new Join(BaseQuery.From(query));
        }

        public Join JoinWith(Func<Query, Query> callback)
        {
            return new Join(BaseQuery.From(callback));
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
            return new Join(BaseQuery.AddComponent("where", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = BaseQuery.GetOr(),
                IsNot = BaseQuery.GetNot()
            }));
        }

        public Join OrOn(string first, string second, string op = "=")
        {
            return new Join(BaseQuery.Or()).On(first, second, op);
        }

    }
}
