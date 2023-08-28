using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractFrom : AbstractClause
    {
        /// <summary>
        ///     Try to extract the Alias for the current clause.
        /// </summary>
        /// <returns></returns>
        public required string? Alias { get; init; }
    }

    /// <summary>
    ///     Represents a "from" clause.
    /// </summary>
    public sealed class FromClause : AbstractFrom
    {
        public required string Table { get; init; }
    }

    /// <summary>
    ///     Represents a "from subQuery" clause.
    /// </summary>
    public sealed class QueryFromClause : AbstractFrom
    {
        public required Query Query { get; init; }
    }

    public sealed class RawFromClause : AbstractFrom
    {
        public required string Expression { get; init; }
        public required ImmutableArray<object> Bindings { get; init; }
    }

    /// <summary>
    ///     Represents a FROM clause that is an ad-hoc table built with predefined values.
    /// </summary>
    public sealed class AdHocTableFromClause : AbstractFrom
    {
        public required ImmutableArray<string> Columns { get; init; }
        public required ImmutableArray<object?> Values { get; init; }
    }
}
