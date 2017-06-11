using System;
using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractColumn : AbstractClause
    {
    }

    public class Column : AbstractColumn
    {
        public string Name { get; set; }

        public override AbstractClause Clone()
        {
            return new Column
            {
                Name = Name,
                Component = Component,
            };
        }
    }

    public class QueryColumn : AbstractColumn
    {
        public Query Query { get; set; }
        public override object[] Bindings => Query.Bindings.ToArray();

        public override AbstractClause Clone()
        {
            return new QueryColumn
            {
                Query = Query.Clone(),
                Component = Component,
            };
        }
    }

    public class RawColumn : AbstractColumn, RawInterface
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
            return new RawColumn
            {
                Expression = Expression,
                Bindings = _bindings,
                Component = Component,
            };
        }
    }

}