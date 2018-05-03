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

        /// <summary>
        /// Gets bindings that used for the specified <paramref name="engine"/> in current clause.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <returns>Values to bind in current clause.</returns>
        public virtual object[] GetBindings(string engine)
        {
            return new object[] { };
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public abstract AbstractClause Clone();
    }
}