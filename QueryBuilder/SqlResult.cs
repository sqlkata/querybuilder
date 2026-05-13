using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SqlKata
{
    /// <summary>
    /// Represents the SQL text and bindings produced by compiling a <see cref="Query"/>.
    /// </summary>
    public class SqlResult
    {
        private string ParameterPlaceholder { get; set; }
        private string EscapeCharacter { get; set; }

        /// <summary>
        /// Initializes a new SQL result with the placeholder and escape character used by the compiler.
        /// </summary>
        /// <param name="parameterPlaceholder">The placeholder token used for parameter bindings.</param>
        /// <param name="escapeCharacter">The escape character used while replacing placeholders.</param>
        public SqlResult(string parameterPlaceholder, string escapeCharacter)
        {
            ParameterPlaceholder = parameterPlaceholder;
            EscapeCharacter = escapeCharacter;
        }

        /// <summary>
        /// Gets or sets the query that produced this result.
        /// </summary>
        public Query Query { get; set; }

        /// <summary>
        /// Gets or sets the raw SQL text before bindings are expanded.
        /// </summary>
        public string RawSql { get; set; } = "";

        /// <summary>
        /// Gets or sets the ordered binding values for the compiled SQL.
        /// </summary>
        public List<object> Bindings { get; set; } = new List<object>();

        /// <summary>
        /// Gets or sets the compiled SQL text.
        /// </summary>
        public string Sql { get; set; } = "";

        /// <summary>
        /// Stores bindings by name for compilers that support named parameters.
        /// </summary>
        public Dictionary<string, object> NamedBindings = new Dictionary<string, object>();

        private static readonly Type[] NumberTypes =
        {
            typeof(int),
            typeof(long),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(short),
            typeof(ushort),
            typeof(ulong),
        };

        /// <summary>
        /// Returns the raw SQL with binding values inlined for inspection and debugging.
        /// </summary>
        /// <returns>The SQL text with bindings converted to SQL literals.</returns>
        public override string ToString()
        {
            var deepParameters = Helper.Flatten(Bindings).ToList();

            var subject = Helper.ReplaceAll(RawSql, ParameterPlaceholder, EscapeCharacter, i =>
            {
                if (i >= deepParameters.Count)
                {
                    throw new Exception(
                        $"Failed to retrieve a binding at index {i}, the total bindings count is {Bindings.Count}");
                }

                var value = deepParameters[i];
                return ChangeToSqlValue(value);
            });

            return Helper.RemoveEscapeCharacter(subject, ParameterPlaceholder, EscapeCharacter);
        }

        private string ChangeToSqlValue(object value)
        {
            if (value == null)
            {
                return "NULL";
            }

            if (Helper.IsArray(value))
            {
                return Helper.JoinArray(",", value as IEnumerable);
            }

            if (NumberTypes.Contains(value.GetType()))
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            if (value is DateTime date)
            {
                if (date.Date == date)
                {
                    return "'" + date.ToString("yyyy-MM-dd") + "'";
                }

                return "'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            }

            if (value is bool vBool)
            {
                return vBool ? "true" : "false";
            }

            if (value is Enum vEnum)
            {
                return Convert.ToInt32(vEnum) + $" /* {vEnum} */";
            }

            // fallback to string
            return "'" + value.ToString().Replace("'","''") + "'";
        }
    }
}
