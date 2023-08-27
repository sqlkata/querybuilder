namespace SqlKata
{
    public abstract class AbstractCondition : AbstractClause
    {
        public bool IsOr { get; set; }
        public bool IsNot { get; set; }
    }

    /// <summary>
    ///     Represents a comparison between a column and a value.
    /// </summary>
    public class BasicCondition : AbstractCondition
    {
        public string Column { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }
    }

    public class BasicStringCondition : BasicCondition
    {
        private string? _escapeCharacter;

        public bool CaseSensitive { get; set; }

        public string? EscapeCharacter
        {
            get => _escapeCharacter;
            init
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = null;
                else if (value.Length > 1)
                    throw new ArgumentOutOfRangeException(
                        $"The {nameof(EscapeCharacter)} can only contain a single character!");
                _escapeCharacter = value;
            }
        }
    }

    public class BasicDateCondition : BasicCondition
    {
        public string Part { get; set; }
    }

    /// <summary>
    ///     Represents a comparison between two columns.
    /// </summary>
    public class TwoColumnsCondition : AbstractCondition
    {
        public string First { get; set; }
        public string Operator { get; set; }
        public string Second { get; set; }
    }

    /// <summary>
    ///     Represents a comparison between a column and a full "subQuery".
    /// </summary>
    public class QueryCondition<T> : AbstractCondition where T : Query
    {
        public string Column { get; set; }
        public string Operator { get; set; }
        public Query Query { get; set; }
    }

    /// <summary>
    ///     Represents a comparison between a full "subQuery" and a value.
    /// </summary>
    public class SubQueryCondition<T> : AbstractCondition where T : Query
    {
        public object Value { get; set; }
        public string Operator { get; set; }
        public Query Query { get; set; }
    }

    /// <summary>
    ///     Represents a "is in" condition.
    /// </summary>
    public class InCondition<T> : AbstractCondition
    {
        public string Column { get; set; }
        public IEnumerable<T> Values { get; set; }
    }

    /// <summary>
    ///     Represents a "is in subQuery" condition.
    /// </summary>
    public class InQueryCondition : AbstractCondition
    {
        public Query Query { get; set; }
        public string Column { get; set; }
    }

    /// <summary>
    ///     Represents a "is between" condition.
    /// </summary>
    public class BetweenCondition<T> : AbstractCondition
    {
        public string Column { get; set; }
        public T Higher { get; set; }
        public T Lower { get; set; }
    }

    /// <summary>
    ///     Represents an "is null" condition.
    /// </summary>
    public class NullCondition : AbstractCondition
    {
        public string Column { get; set; }
    }

    /// <summary>
    ///     Represents a boolean (true/false) condition.
    /// </summary>
    public class BooleanCondition : AbstractCondition
    {
        public string Column { get; set; }
        public bool Value { get; set; }
    }

    /// <summary>
    ///     Represents a "nested" clause condition.
    ///     i.e OR (myColumn = "A")
    /// </summary>
    public class NestedCondition<T> : AbstractCondition 
    {
        public Query Query { get; set; }
    }

    /// <summary>
    ///     Represents an "exists sub query" clause condition.
    /// </summary>
    public class ExistsCondition : AbstractCondition
    {
        public Query Query { get; set; }
    }

    public class RawCondition : AbstractCondition
    {
        public string Expression { get; set; }
        public object[] Bindings { set; get; }
    }
}
