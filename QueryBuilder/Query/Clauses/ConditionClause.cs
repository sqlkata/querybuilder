using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractCondition : AbstractClause
    {
        public required bool IsOr { get; init; }
        public required bool IsNot { get; init; }
    }

    /// <summary>
    ///     Represents a comparison between a column and a value.
    /// </summary>
    public class BasicCondition : AbstractCondition
    {
        public required string Column { get; init; }
        public required string Operator { get; init; }
        public required object Value { get; init; }
    }

    public class BasicStringCondition : BasicCondition
    {
        public required bool CaseSensitive { get; init; }
        public required char? EscapeCharacter { get; init; }
    }

    public sealed class BasicDateCondition : BasicCondition
    {
        public required string Part { get; init; }
    }

    /// <summary>
    ///     Represents a comparison between two columns.
    /// </summary>
    public sealed class TwoColumnsCondition : AbstractCondition
    {
        public required string First { get; init; }
        public required string Operator { get; init; }
        public required string Second { get; init; }
    }

    /// <summary>
    ///     Represents a comparison between a column and a full "subQuery".
    /// </summary>
    public sealed class QueryCondition : AbstractCondition
    {
        public required string Column { get; init; }
        public required string Operator { get; init; }
        public required Query Query { get; init; }
    }

    /// <summary>
    ///     Represents a comparison between a full "subQuery" and a value.
    /// </summary>
    public class SubQueryCondition : AbstractCondition
    {
        public required object Value { get; init; }
        public required string Operator { get; init; }
        public required Query Query { get; init; }
    }

    /// <summary>
    ///     Represents a "is in" condition.
    /// </summary>
    public class InCondition : AbstractCondition
    {
        public required string Column { get; init; }
        public required ImmutableArray<object> Values { get; init; }
    }

    /// <summary>
    ///     Represents a "is in subQuery" condition.
    /// </summary>
    public class InQueryCondition : AbstractCondition
    {
        public required Query Query { get; init; }
        public required string Column { get; init; }
    }

    /// <summary>
    ///     Represents a "is between" condition.
    /// </summary>
    public class BetweenCondition : AbstractCondition
    {
        public required string Column { get; init; }
        public required object Higher { get; init; }
        public required object Lower { get; init; }
    }

    /// <summary>
    ///     Represents an "is null" condition.
    /// </summary>
    public class NullCondition : AbstractCondition
    {
        public required string Column { get; init; }
    }

    /// <summary>
    ///     Represents a boolean (true/false) condition.
    /// </summary>
    public class BooleanCondition : AbstractCondition
    {
        public required string Column { get; init; }
        public required bool Value { get; init; }
    }

    /// <summary>
    ///     Represents a "nested" clause condition.
    ///     i.e OR (myColumn = "A")
    /// </summary>
    public class NestedCondition : AbstractCondition 
    {
        public required Query Query { get; init; }
    }

    /// <summary>
    ///     Represents an "exists sub query" clause condition.
    /// </summary>
    public class ExistsCondition : AbstractCondition
    {
        public required Query Query { get; init; }
    }

    public class RawCondition : AbstractCondition
    {
        public required string Expression { get; init; }
        public required object[] Bindings { get; init; }
    }
}
