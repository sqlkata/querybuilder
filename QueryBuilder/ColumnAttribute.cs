using System;

namespace SqlKata
{
    /// <summary>
    /// This class is used as metadata on a property to generate different name in the output query.
    /// </summary>
    public class ColumnAttribute : Attribute
    {
        public string Name { get; private set; }
        public ColumnAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Name parameter is required");
            }
            Name = name;
        }

    }
}
