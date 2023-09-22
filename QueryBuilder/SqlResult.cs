using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SqlKata
{
    public class SqlResult
    {
        public SqlResult() { }

        public Query Query { get; set; }
        public string RawSql { get; set; } = "";
        public List<object> Bindings { get; set; } = new List<object>();
        public string Sql { get; set; } = "";
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

        /// <inheritdoc/>
        public override string ToString()
        {
            var deepParameters = Helper.Flatten(Bindings).ToList();

            return Helper.ReplaceAll(RawSql, "?", i =>
            {
                if (i >= deepParameters.Count)
                {
                    throw new Exception(
                        $"Failed to retrieve a binding at index {i}, the total bindings count is {Bindings.Count}");
                }

                var value = deepParameters[i];
                return ChangeToSqlValue(value);
            });
        }

        /// <summary>
        /// Convert each value to its string representation
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <returns>string representation of the <paramref name="value"/></returns>
        protected virtual string ChangeToSqlValue(object value)
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
            return WrapStringValue(value.ToString());
        }

        /// <summary>
        /// Wrap a string value with identifiers
        /// </summary>
        /// <param name="value">string parameter</param>
        /// <returns></returns>
        /// <remarks>
        /// Default Functionality wraps in single quotes
        /// <br/>   value --> 'value'
        /// <br/> 'value' --> ''value'''
        /// </remarks>
        protected virtual string WrapStringValue(string value)
        {
            return "'" + value.ToString().Replace("'", "''") + "'";
        }

    }
}
