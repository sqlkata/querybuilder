using System;
using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractFrom : AbstractClause
    {

    }

    /// <summary>
    /// Represents a "from" clause.
    /// </summary>
    public class From : AbstractFrom
    {
        public string Table { get; set; }

        public override AbstractClause Clone()
        {
            return new From
            {
                Table = Table,
                Component = Component,
            };
        }
    }

    /// <summary>
    /// Represents a "from subquery" clause.
    /// </summary>
    public class QueryFrom : AbstractFrom
    {
        public Query Query { get; set; }
        public override object[] Bindings => Query.Bindings.ToArray();

        public override AbstractClause Clone()
        {
            return new QueryFrom
            {
                Query = Query.Clone(),
                Component = Component,
            };
        }
    }

    public class RawFrom : AbstractFrom, RawInterface
    {
        protected object[] _bindings;
        public string Expression { get; set; }
        public override object[] Bindings
        {
            get => _bindings;
            set
            {
                _bindings = value;
            }
        }

        public override AbstractClause Clone()
        {
            return new RawFrom
            {
                Expression = Expression,
                Bindings = _bindings,
                Component = Component,
            };
        }
    }

}