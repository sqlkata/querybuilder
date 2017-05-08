using System;
using System.Collections.Generic;

namespace SqlKata
{
    public class AbstractFrom : AbstractClause
    {

    }

    /// <summary>
    /// Represents a "from" clause.
    /// </summary>
    public class From : AbstractFrom
    {
        public string Table { get; set; }
    }

    /// <summary>
    /// Represents a "from subquery" clause.
    /// </summary>
    public class QueryFrom : AbstractFrom
    {
        public Query Query { get; set; }
        public override object[] Bindings => Query.Bindings.ToArray();
    }

    public class RawFrom : AbstractFrom, RawInterface
    {
        protected object[] _bindings;
        public string Expression { get; set; }
        public override object[] Bindings
        {
            get
            {
                return _bindings;
            }
            set
            {
                _bindings = value;
            }
        }
    }

}