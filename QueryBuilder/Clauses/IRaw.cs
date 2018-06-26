namespace SqlKata
{
    /// <summary>
    /// The interface to use when defining a part of a sql query in it's raw form
    /// </summary>
    public interface IRaw
    {
        /// <summary>
        /// Returns the RAW sql code 
        /// </summary>
        string Expression { get; }

        /// <summary>
        /// Returns the bindings to use on the RAW <see cref="Expression"/>
        /// </summary>
        object[] Bindings { get; }
    }
}