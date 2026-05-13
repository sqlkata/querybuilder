namespace SqlKata
{
    /// <summary>
    /// Represents a raw SQL literal that should be inserted without parameter binding.
    /// </summary>
    public class UnsafeLiteral
    {
        /// <summary>
        /// Gets or sets the literal SQL value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new unsafe SQL literal.
        /// </summary>
        /// <param name="value">The raw SQL value.</param>
        /// <param name="replaceQuotes">Whether single quotes should be escaped in the value.</param>
        public UnsafeLiteral(string value, bool replaceQuotes = true)
        {
            if (value == null)
            {
                value = "";
            }

            if (replaceQuotes)
            {
                value = value.Replace("'", "''");
            }

            this.Value = value;
        }

    }
}
