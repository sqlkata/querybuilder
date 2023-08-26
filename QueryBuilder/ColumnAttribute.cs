using System;
using System.Runtime.CompilerServices;

namespace SqlKata
{
    /// <summary>
    ///     This class is used as metadata on a property to generate different name in the output query.
    /// </summary>
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public string Name { get; private set; }
    }

    /// <summary>
    ///     This class is used as metadata on a property to determine if it is a primary key
    /// </summary>
    public class KeyAttribute : ColumnAttribute
    {
        public KeyAttribute([CallerMemberName] string name = "")
            : base(name)
        {
        }
    }
}
