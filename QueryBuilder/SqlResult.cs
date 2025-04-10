using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SqlKata
{
    public class SqlResult
    {
        private string ParameterPlaceholder { get; set; }
        private string EscapeCharacter { get; set; }
        public SqlResult(string parameterPlaceholder, string escapeCharacter)
        {
            ParameterPlaceholder = parameterPlaceholder;
            EscapeCharacter = escapeCharacter;
        }
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
                if (value is NamedParameterVariable v)
                {
                    return ChangeToSqlValue(v.Value);
                }
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
