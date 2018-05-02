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
                Engine = Engine,
                Name = Name,
                Component = Component,
            };
        }
    }

    public class QueryColumn : AbstractColumn
    {
        public Query Query { get; set; }
        public override object[] GetBindings(string engine)
        {
            return Query.GetBindings(engine).ToArray();
        }

        public override AbstractClause Clone()
        {
            return new QueryColumn
            {
                Engine = Engine,
                Query = Query.Clone(),
                Component = Component,
            };
        }
    }

    public class RawColumn : AbstractColumn, IRaw
    {
        public string Expression { get; set; }
        protected object[] _bindings;
        public object[] Bindings { set => _bindings = value; }
        public override object[] GetBindings(string engine)
        {
            return _bindings;
        }

        public override AbstractClause Clone()
        {
            return new RawColumn
            {
                Engine = Engine,
                Expression = Expression,
                _bindings = _bindings,
                Component = Component,
            };
        }
    }

}