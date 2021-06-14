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

        public AbstractClause()
        {
        }

        public AbstractClause(AbstractClause other)
        {
            Engine = other.Engine;
            Component = other.Component;
        }

        public abstract AbstractClause Clone();
    }
}
