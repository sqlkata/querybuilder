using System;
using System.Collections.Generic;

namespace SqlKata
{
    public class AbstractColumn : AbstractClause
    {
    }

    public class Column : AbstractColumn
    {
        public string Name { get; set; }
    }

    public class QueryColumn : AbstractColumn
    {
        public Query Query { get; set; }
        public override object[] Bindings => Query.Bindings.ToArray();
    }

    public class RawColumn : AbstractColumn, RawInterface
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