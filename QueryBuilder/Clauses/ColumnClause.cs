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

    public enum FallbackType
    {
        Query,
        Column,
        Value
    }

    public class CoalesceFallback
    {
        public FallbackType Type { get;set; }
        public object Value { get;set; }

        public CoalesceFallback(object value, FallbackType type)
        {
            Value = value;
            Type = type;
        }

        public CoalesceFallback(Query query)
        {
            Value = query;
            Type = FallbackType.Query;
        }

        public CoalesceFallback(Func<Query, Query> callback)
        {
            Value = callback.Invoke(new Query());
            Type = FallbackType.Query;
        }
    }

    public class CoalesceColumn : AbstractColumn
    {
        public Query Query { get; set; }
        public List<CoalesceFallback> Fallbacks { get; set; }

        public override object[] GetBindings(string engine)
        {
            return Query.GetBindings(engine).ToArray();
        }

        public override AbstractClause Clone()
        {
            return new CoalesceColumn
            {
                Engine = Engine,
                Query = Query.Clone(),
                Component = Component,
                Fallbacks = Fallbacks
            };
        }
    }

    public class RawColumn : AbstractColumn, RawInterface
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