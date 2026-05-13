namespace SqlKata
{
    /// <summary>
    /// Describes a related query that should be loaded together with the parent query result.
    /// </summary>
    public class Include
    {
        /// <summary>
        /// Gets or sets the include name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the query used to load the related records.
        /// </summary>
        public Query Query { get; set; }

        /// <summary>
        /// Gets or sets the foreign key used to relate the included query to the parent query.
        /// </summary>
        public string ForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the local key used to relate the parent query to the included query.
        /// </summary>
        public string LocalKey { get; set; }

        /// <summary>
        /// Gets or sets whether the include represents multiple related records.
        /// </summary>
        public bool IsMany { get; set; }

        /// <summary>
        /// Creates a copy of the include definition.
        /// </summary>
        /// <returns>A cloned include definition.</returns>
        public Include Clone()
        {
            return new Include
            {
                Name = Name,
                Query = Query.Clone(),
                ForeignKey = ForeignKey,
                LocalKey = LocalKey,
                IsMany = IsMany,
            };
        }
    }
}
