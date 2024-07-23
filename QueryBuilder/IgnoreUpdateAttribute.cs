using System;

namespace SqlKata
{
    /// <summary>
    /// This class is used as metadata to ignore a property on update queries
    /// </summary>
    /// <example>
    /// <code>
    /// public class  Person
    /// {
    ///    public string Name {get ;set;}
    ///
    ///    [IgnoreUpdate]
    ///    public string PhoneNumber {get ;set;}
    ///
    /// }
    ///
    /// new Query("Table").Update(new Person { Name = "User", PhoneNumber = "70123456" })
    ///
    /// output: UPDATE INTO [Table] ([Name]) VALUES('User')
    /// </code>
    /// </example>
    public class IgnoreUpdateAttribute : Attribute
    {
    }
}
