using System.Collections.Generic;

namespace SqlKata
{
    /// <summary>
    ///     Represents an abstract class as base definition for an SQL condition,
    ///     e.g. not, and, ord, is, &lt;, &gt;, etc...
    /// </summary>
    public abstract class AbstractCondition : AbstractClause
    {
        #region Properties
        /// <summary>
        ///     Returns <c>true</c> when this is an OR
        /// </summary>
        public bool IsOr { get; internal set; }

        /// <summary>
        ///     Returns <c>true</c> when this is an NOT
        /// </summary>
        public bool IsNot { get; internal set; }
        #endregion
    }

    /// <summary>
    ///     Represents a comparison between a column and a value.
    /// </summary>
    public class BasicCondition<T> : AbstractCondition
    {
        #region Properties
        /// <summary>
        ///     The column
        /// </summary>
        public string Column { get; internal set; }

        /// <summary>
        ///     The operator
        /// </summary>
        public string Operator { get; internal set; }

        /// <summary>
        ///     The value
        /// </summary>
        public virtual T Value { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BasicCondition<T>
            {
                Engine = Engine,
                Column = Column,
                Operator = Operator,
                Value = Value,
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component
            };
        }
        #endregion
    }

    /// <inheritdoc />
    public class BasicStringCondition : BasicCondition<string>
    {
        #region Properties
        /// <summary>
        /// Returns <c>true</c> when case-sensitive
        /// </summary>
        public bool CaseSensitive { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BasicStringCondition
            {
                Engine = Engine,
                Column = Column,
                Operator = Operator,
                Value = Value,
                IsOr = IsOr,
                IsNot = IsNot,
                CaseSensitive = CaseSensitive,
                Component = Component
            };
        }
        #endregion
    }

    /// <inheritdoc />
    public class BasicDateCondition : BasicCondition<object>
    {
        #region Properties
        public string Part { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BasicDateCondition
            {
                Engine = Engine,
                Column = Column,
                Operator = Operator,
                Value = Value,
                IsOr = IsOr,
                IsNot = IsNot,
                Part = Part,
                Component = Component
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents a comparison between two columns.
    /// </summary>
    public class TwoColumnsCondition : AbstractCondition
    {
        #region Properties
        public string First { get; internal set; }
        public string Operator { get; internal set; }
        public string Second { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new TwoColumnsCondition
            {
                Engine = Engine,
                First = First,
                Operator = Operator,
                Second = Second,
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents a comparison between a column and a full "subquery".
    /// </summary>
    public class QueryCondition<T> : AbstractCondition where T : BaseQuery<T>
    {
        #region Properties
        public string Column { get; internal set; }
        public string Operator { get; internal set; }
        public Query Query { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new QueryCondition<T>
            {
                Engine = Engine,
                Column = Column,
                Operator = Operator,
                Query = Query.Clone(),
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents a "is in" condition.
    /// </summary>
    public class InCondition<T> : AbstractCondition
    {
        #region Properties
        public string Column { get; internal set; }
        public IEnumerable<T> Values { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new InCondition<T>
            {
                Engine = Engine,
                Column = Column,
                Values = new List<T>(Values),
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents a "is in subquery" condition.
    /// </summary>
    public class InQueryCondition : AbstractCondition
    {
        #region Properties
        public Query Query { get; internal set; }
        public string Column { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new InQueryCondition
            {
                Engine = Engine,
                Column = Column,
                Query = Query.Clone(),
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents a "is between" condition.
    /// </summary>
    public class BetweenCondition<T> : AbstractCondition
    {
        #region Properties
        public string Column { get; internal set; }
        public T Higher { get; internal set; }
        public T Lower { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BetweenCondition<T>
            {
                Engine = Engine,
                Column = Column,
                Higher = Higher,
                Lower = Lower,
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents an "is null" condition.
    /// </summary>
    public class NullCondition : AbstractCondition
    {
        #region Properties
        public string Column { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new NullCondition
            {
                Engine = Engine,
                Column = Column,
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents a "nested" clause condition.
    ///     i.e OR (myColumn = "A")
    /// </summary>
    public class NestedCondition<T> : AbstractCondition where T : BaseQuery<T>
    {
        #region Properties
        public T Query { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new NestedCondition<T>
            {
                Engine = Engine,
                Query = Query.Clone(),
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents an "exists sub query" clause condition.
    /// </summary>
    public class ExistsCondition<T> : AbstractCondition where T : BaseQuery<T>
    {
        #region Properties
        public T Query { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new ExistsCondition<T>
            {
                Engine = Engine,
                Query = Query.Clone(),
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents an "exists sub query" clause condition in it's
    ///     RAW form with it's own expression and bindings
    /// </summary>
    public class RawCondition : AbstractCondition, IRaw
    {
        #region Properties
        /// <inheritdoc />
        public string Expression { get; internal set; }

        /// <inheritdoc />
        public object[] Bindings { internal set; get; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawCondition
            {
                Engine = Engine,
                Expression = Expression,
                Bindings = Bindings,
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component
            };
        }
        #endregion
    }
}