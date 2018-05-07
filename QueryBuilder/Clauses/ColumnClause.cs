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

    public class RawColumn : AbstractColumn, RawInterface
    {
        public string Expression { get; set; }
        public object[] Bindings { set; get; }

        public override AbstractClause Clone()
        {
            return new RawColumn
            {
                Engine = Engine,
                Expression = Expression,
                Bindings = Bindings,
                Component = Component,
            };
        }
    }

}