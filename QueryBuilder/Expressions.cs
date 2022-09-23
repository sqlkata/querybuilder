namespace SqlKata
{
    public static class Expressions
    {
        /// <summary>
        /// Instruct the compiler to resolve the value from the predefined variables
        /// In the current query or parents queries.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Variable Variable(string name)
        {
            return new Variable(name);
        }

        /// <summary>
        /// Instruct the compiler to treat this as a literal.
        /// WARNING: don't pass user data directly to this method.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="replaceQuotes">if true it will esacpe single quotes</param>
        /// <returns></returns>
        public static UnsafeLiteral UnsafeLiteral(string value, bool replaceQuotes = true)
        {
            return new UnsafeLiteral(value, replaceQuotes);
        }
    }
}
