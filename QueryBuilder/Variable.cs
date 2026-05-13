namespace SqlKata
{
    /// <summary>
    /// Represents a named variable placeholder that can be resolved by a query compiler.
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Gets or sets the variable name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new variable placeholder.
        /// </summary>
        /// <param name="name">The variable name.</param>
        public Variable(string name)
        {
            this.Name = name;
        }

    }
}
