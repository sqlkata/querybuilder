using System;

namespace SqlKata
{
    /// <summary>
    /// This class is used as metadata to ignore a property on insert and update queries
    /// </summary>
    /// <example>
    /// <code>
    /// public class  Person
    /// {
    ///    public string Name {get ;set;}
    ///
    ///    [Ignore]
    ///    public string PhoneNumber {get ;set;}
    ///
    /// }
    ///
    /// new Query("Table").Insert(new Person { Name = "User", PhoneNumber = "70123456" })
    ///
    /// output: INSERT INTO [Table] ([Name]) VALUES('User')
    /// </code>
    /// </example>
    public class IgnoreAttribute : Attribute
    {
    }
}
