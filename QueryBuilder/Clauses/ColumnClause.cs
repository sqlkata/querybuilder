using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractColumn : AbstractClause
    {
    }

    /// <summary>
    ///     Represents "column" or "column as alias" clause.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public sealed class Column : AbstractColumn
    {
        /// <summary>
        ///     Gets or sets the column name. Can be "columnName" or "columnName as columnAlias".
        /// </summary>
        /// <value>
        ///     The column name.
        /// </value>
        public required string Name { get; init; }
    }

    /// <summary>
    ///     Represents column clause calculated using query.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public sealed class QueryColumn : AbstractColumn
    {
        /// <summary>
        ///     Gets or sets the query that will be used for column value calculation.
        /// </summary>
        /// <value>
        ///     The query for column value calculation.
        /// </value>
        public required Query Query { get; init; }
    }

    public sealed class RawColumn : AbstractColumn
    {
        /// <summary>
        ///     Gets or sets the RAW expression.
        /// </summary>
        /// <value>
        ///     The RAW expression.
        /// </value>
        public required string Expression { get; init; }

        public required ImmutableArray<object> Bindings { get; init; }
    }

    /// <summary>
    ///     Represents an aggregated column clause with an optional filter
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public sealed class AggregatedColumn : AbstractColumn
    {
        /// <summary>
        ///     Gets or sets the a query that used to filter the data,
        ///     the compiler will consider only the `Where` clause.
        /// </summary>
        /// <value>
        ///     The filter query.
        /// </value>
        public required Query? Filter { get; init; }

        public required string Aggregate { get; init; }
        public required AbstractColumn Column { get; init; }
    }
}
