namespace SqlKata
{
    public abstract class AbstractClause
    {
        /// <summary>
        /// Gets or sets the SQL engine.
        /// </summary>
        /// <value>
        /// The SQL engine.
        /// </value>
        public string Engine { get; set; } = null;

        /// <summary>
        /// Gets or sets the component name.
        /// </summary>
        /// <value>
        /// The component name.
        /// </value>
        public string Component { get; set; }
        public abstract AbstractClause Clone();
    }
}