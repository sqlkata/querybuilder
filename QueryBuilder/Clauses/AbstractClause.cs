namespace SqlKata
{
    /// <summary>
    ///     Represents an abstract class as base definition for an SQL clause,
    ///     e.g. constraint, update, from, group by, having, order by, etc...
    /// </summary>
    public abstract class AbstractClause
    {
        #region Properties
        /// <summary>
        ///     Gets or sets the SQL engine.
        /// </summary>
        /// <value>
        ///     The SQL engine.
        /// </value>
        public string Engine { get; internal set; } = null;

        /// <summary>
        ///     Gets or sets the component name.
        /// </summary>
        /// <value>
        ///     The component name.
        /// </value>
        public string Component { get; internal set; }
        #endregion

        #region Clone
        /// <summary>
        ///     Returns a clone of this object
        /// </summary>
        /// <returns></returns>
        public abstract AbstractClause Clone();
        #endregion
    }
}