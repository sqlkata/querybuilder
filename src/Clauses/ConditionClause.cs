using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public class AbstractCondition : AbstractClause
    {
        public bool IsOr { get; set; } = false;
        public bool IsNot { get; set; } = false;
    }

    /// <summary>
    /// Represents a comparison between a column and a value.
    /// </summary>
    public class BasicCondition<T> : AbstractCondition
    {
        public string Column { get; set; }
        public string Operator { get; set; }
        public virtual T Value { get; set; }
        public override object[] Bindings => new object[] { Value };
    }

    public class BasicStringCondition : BasicCondition<string>
    {
        public bool CaseSensitive { get; set; } = false;
    }

    /// <summary>
    /// Represents a comparison between two columns.
    /// </summary>
    public class TwoColumnsCondition : AbstractCondition
    {
        public string First { get; set; }
        public string Operator { get; set; }
        public string Second { get; set; }

    }

    /// <summary>
    /// Represents a comparison between a column and a full "subquery". 
    /// </summary>
    public class QueryCondition<T> : AbstractCondition where T : BaseQuery<T>
    {
        public string Column { get; set; }
        public string Operator { get; set; }
        public Query Query { get; set; }
        public override object[] Bindings => Query.Bindings.ToArray();
    }

    /// <summary>
    /// Represents a "is in" condition.
    /// </summary>
    public class InCondition<T> : AbstractCondition
    {
        public string Column { get; set; }
        public List<T> Values { get; set; }
        public override object[] Bindings => Values.Select(x => x as object).ToArray();
    }

    /// <summary>
    /// Represents a "is in subquery" condition.
    /// </summary>
    public class InQueryCondition : AbstractCondition
    {
        public string Column { get; set; }
        public Query Query { get; set; }
        public override object[] Bindings => Query.Bindings.ToArray();
    }

    /// <summary>
    /// Represents a "is between" condition.
    /// </summary>
    public class BetweenCondition<T> : AbstractCondition
    {
        public string Column { get; set; }
        public T Higher { get; set; }
        public T Lower { get; set; }
        public override object[] Bindings => new object[] { Lower, Higher };
    }

    /// <summary>
    /// Represents an "is null" condition.
    /// </summary>
    public class NullCondition : AbstractCondition
    {
        public string Column { get; set; }
    }

    /// <summary>
    /// Represents a "nested" clause condition.
    /// i.e OR (ColA = "A")
    /// </summary>
    public class NestedCondition<T> : AbstractCondition where T : BaseQuery<T>
    {
        public T Query { get; set; }
        public override object[] Bindings => Query.Bindings.ToArray();
    }

    /// <summary>
    /// Represents an "exists sub query" clause condition.
    /// </summary>
    public class ExistsCondition<T> : AbstractCondition where T : BaseQuery<T>
    {
        public T Query { get; set; }
        public override object[] Bindings => Query.Bindings.ToArray();
    }

    public class RawCondition : AbstractCondition, RawInterface
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