using System;

namespace SqlKata
{
    /// <summary>
    /// This class is used as metadata to ignore a property on an object in order to exclude it from query
    /// </summary>
    /// <example>
    /// <code>
    /// public class  Person  
    /// { 
    ///    public string Name {get ;set;}
    ///    [Ignore]
    ///    public string PhoneNumber {get ;set;}
    ///
    /// }
    ///
    /// output: SELECT [Name] FROM [Person]
    /// </code>
    /// </example>
    public class IgnoreAttribute : Attribute
    {
    }
}
