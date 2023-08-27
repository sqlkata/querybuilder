namespace SqlKata
{
    public abstract class AbstractColumn : AbstractClause
    {
    }

    /// <summary>
    ///     Represents "column" or "column as alias" clause.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class Column : AbstractColumn
    {
        /// <summary>
        ///     Gets or sets the column name. Can be "columnName" or "columnName as columnAlias".
        /// </summary>
        /// <value>
        ///     The column name.
        /// </value>
        public string Name { get; set; }
    }

    /// <summary>
    ///     Represents column clause calculated using query.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class QueryColumn : AbstractColumn
    {
        /// <summary>
        ///     Gets or sets the query that will be used for column value calculation.
        /// </summary>
        /// <value>
        ///     The query for column value calculation.
        /// </value>
        public Query Query { get; set; }
    }

    public class RawColumn : AbstractColumn
    {
        /// <summary>
        ///     Gets or sets the RAW expression.
        /// </summary>
        /// <value>
        ///     The RAW expression.
        /// </value>
        public string Expression { get; set; }

        public object[] Bindings { set; get; }
    }

    /// <summary>
    ///     Represents an aggregated column clause with an optional filter
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class AggregatedColumn : AbstractColumn
    {
        /// <summary>
        ///     Gets or sets the a query that used to filter the data,
        ///     the compiler will consider only the `Where` clause.
        /// </summary>
        /// <value>
        ///     The filter query.
        /// </value>
        public Query? Filter { get; set; }

        public string Aggregate { get; set; }
        public AbstractColumn Column { get; set; }
    }
}
