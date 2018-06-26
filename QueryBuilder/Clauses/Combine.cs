namespace SqlKata
{
    public abstract class AbstractCombine : AbstractClause
    {
    }

    public class Combine : AbstractCombine
    {
        #region Properties
        /// <summary>
        ///     Gets or sets the query to be combined with.
        /// </summary>
        /// <value>
        ///     The query that will be combined.
        /// </value>
        public Query Query { get; internal set; }

        /// <summary>
        ///     Gets or sets the combine operation, e.g. "UNION", etc.
        /// </summary>
        /// <value>
        ///     The combine operation.
        /// </value>
        public string Operation { get; internal set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Combine" /> clause will combine all.
        /// </summary>
        /// <value>
        ///     <c>true</c> if all; otherwise, <c>false</c>.
        /// </value>
        public bool All { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new Combine
            {
                Engine = Engine,
                Operation = Operation,
                Component = Component,
                Query = Query,
                All = All
            };
        }
        #endregion
    }

    public class RawCombine : AbstractCombine, IRaw
    {
        #region Properties
        /// <summary>
        /// Returns the expression
        /// </summary>
        public string Expression { get; internal set; }

        /// <summary>
        /// Returns the bindings used in the expression
        /// </summary>
        public object[] Bindings { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawCombine
            {
                Engine = Engine,
                Component = Component,
                Expression = Expression,
                Bindings = Bindings
            };
        }
        #endregion
    }
}