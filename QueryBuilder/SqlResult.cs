using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using System.Text;

namespace SqlKata
{
    public class SqlResult
    {
        public string ParameterPlaceholder { get; set; } = "?";
        public string EscapeCharacter { get; set; } = "\\";

        public SqlResult() { }

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

            if (string.IsNullOrWhiteSpace(RawSql))
            {
                return "";
            }

            if (ParameterPlaceholder.Length != 1 || EscapeCharacter.Length != 1)
            {
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

            var sb = new StringBuilder(RawSql.Length + deepParameters.Count * 8);
            int paramIndex = 0;
            int length = RawSql.Length;
            char escapeChar = EscapeCharacter[0];
            char placeholderChar = ParameterPlaceholder[0];

            for (int i = 0; i < length; i++)
            {
                char c = RawSql[i];

                if (c == escapeChar && i + 1 < length && RawSql[i + 1] == placeholderChar)
                {
                    sb.Append(placeholderChar);
                    i++;
                    continue;
                }

                if (c == '\'')
                {
                    sb.Append('\'');
                    i++;
                    while (i < length)
                    {
                        char sc = RawSql[i];
                        sb.Append(sc);
                        if (sc == '\'')
                        {
                            if (i + 1 < length && RawSql[i + 1] == '\'')
                            {
                                sb.Append('\'');
                                i += 2;
                                continue;
                            }
                            break;
                        }
                        i++;
                    }
                    continue;
                }

                if (c == '-' && i + 1 < length && RawSql[i + 1] == '-')
                {
                    sb.Append("--");
                    i += 2;
                    while (i < length)
                    {
                        char lc = RawSql[i];
                        sb.Append(lc);
                        if (lc == '\n') break;
                        i++;
                    }
                    continue;
                }

                if (c == '/' && i + 1 < length && RawSql[i + 1] == '*')
                {
                    sb.Append("/*");
                    i += 2;
                    while (i < length)
                    {
                        char bc = RawSql[i];
                        sb.Append(bc);
                        if (bc == '*' && i + 1 < length && RawSql[i + 1] == '/')
                        {
                            sb.Append('/');
                            i++;
                            break;
                        }
                        i++;
                    }
                    continue;
                }

                if (c == placeholderChar)
                {
                    if (paramIndex >= deepParameters.Count)
                    {
                        throw new Exception(
                            $"Failed to retrieve a binding at index {paramIndex}, the total bindings count is {Bindings.Count}");
                    }
                    var value = deepParameters[paramIndex++];
                    sb.Append(ChangeToSqlValue(value));
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString();
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
