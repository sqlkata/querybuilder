namespace SqlKata
{
    /// <inheritdoc />
    public abstract class AbstractColumn : AbstractClause
    {
    }

    /// <summary>
    ///     Represents "column" or "column as alias" clause.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class Column : AbstractColumn
    {
        #region Properties
        /// <summary>
        ///     Gets or sets the column name. Can be "columnName" or "columnName as columnAlias".
        /// </summary>
        /// <value>
        ///     The column name.
        /// </value>
        public string Name { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new Column
            {
                Engine = Engine,
                Name = Name,
                Component = Component
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents column clause calculated using query.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class QueryColumn : AbstractColumn
    {
        #region Properties
        /// <summary>
        ///     Gets or sets the query that will be used for column value calculation.
        /// </summary>
        /// <value>
        ///     The query for column value calculation.
        /// </value>
        public Query Query { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new QueryColumn
            {
                Engine = Engine,
                Query = Query.Clone(),
                Component = Component
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents a RAW column with it's own expression and bindings
    /// </summary>
    public class RawColumn : AbstractColumn, IRaw
    {
        #region Properties
        /// <inheritdoc />
        public string Expression { get; internal set; }

        /// <inheritdoc />
        public object[] Bindings { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawColumn
            {
                Engine = Engine,
                Expression = Expression,
                Bindings = Bindings,
                Component = Component
            };
        }
        #endregion
    }
}