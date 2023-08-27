using System.Collections.Immutable;

namespace SqlKata
{
    /// <summary>
    ///     Represents aggregate clause like "COUNT", "MAX" or etc.
    /// </summary>
    /// <seealso cref="AbstractClause" />
    public class AggregateClause : AbstractClause
    {
        /// <summary>
        ///     Gets or sets columns that used in aggregate clause.
        /// </summary>
        /// <value>
        ///     The columns to be aggregated.
        /// </value>
        public required ImmutableArray<string> Columns { get; init; }

        /// <summary>
        ///     Gets or sets the type of aggregate function.
        /// </summary>
        /// <value>
        ///     The type of aggregate function, e.g. "MAX", "MIN", etc.
        /// </value>
        public required string Type { get; init; }
    }
}
