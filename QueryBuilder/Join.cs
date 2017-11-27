using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers;

namespace SqlKata
{
    public class Join : BaseQuery<Join>
    {
        protected string _type = "INNER";

        protected override string[] bindingOrder
        {
            get
            {
                return new[] {
                    "from",
                    "where",
                };
            }
        }

        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value.ToUpper();
            }
        }

        public Join() : base()
        {
        }

        public override Join Clone()
        {
            var clone = base.Clone();
            clone._type = _type;
            return clone;
        }

        public Join AsType(string type)
        {
            Type = type;
            return this;
        }

        /// <summary>
        /// Alias for "from" operator.
        /// Since "from" does not sound well with join clauses
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public Join JoinWith(string table) => From(table);
        public Join JoinWith(Query query) => From(query);
        public Join JoinWith(Func<Query, Query> callback) => From(callback);
        public Join JoinWith(Raw expression) => From(expression);


        public Join AsInner() => AsType("inner");
        public Join AsOuter() => AsType("outer");
        public Join AsLeft() => AsType("left");
        public Join AsRight() => AsType("right");
        public Join AsCross() => AsType("cross");

        public Join On(string first, string second, string op = "=")
        {
            return AddComponent("where", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = getOr(),
                IsNot = getNot()
            });

        }

        public Join OrOn(string first, string second, string op = "=")
        {
            return Or().On(first, second, op);
        }

        public override Join NewQuery()
        {
            return new Join().SetEngineScope(EngineScope);
        }
    }
}