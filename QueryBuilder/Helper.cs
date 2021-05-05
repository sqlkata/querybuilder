using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlKata
{
    public static class Helper
    {
        public static bool IsArray(object value)
        {
            if (value is string)
            {
                return false;
            }

            if (value is byte[])
            {
                return false;
            }

            return value is IEnumerable;
        }

        /// <summary>
        /// Flat IEnumerable one level down
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<object> Flatten(IEnumerable<object> array)
        {
            foreach (var item in array)
            {
                if (IsArray(item))
                {
                    foreach (var sub in (item as IEnumerable))
                    {
                        yield return sub;
                    }
                }
                else
                {
                    yield return item;
                }

            }
        }

        public static IEnumerable<object> FlattenDeep(IEnumerable<object> array)
        {
            return array.SelectMany(o => IsArray(o) ? FlattenDeep(o as IEnumerable<object>) : new[] { o });
        }

        public static IEnumerable<int> AllIndexesOf(string str, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                yield break;
            }

            var index = 0;

            do
            {
                index = str.IndexOf(value, index, StringComparison.Ordinal);

                if (index == -1)
                {
                    yield break;
                }

                yield return index;

            } while ((index += value.Length) < str.Length);
        }

        public static string ReplaceAll(string subject, string match, Func<int, string> callback)
        {
            if (string.IsNullOrWhiteSpace(subject) || !subject.Contains(match))
            {
                return subject;
            }

            var splitted = subject.Split(
                new[] { match },
                StringSplitOptions.None
            );

            return splitted.Skip(1)
                .Select((item, index) => callback(index) + item)
                .Aggregate(splitted.First(), (left, right) => left + right);
        }

        public static string JoinArray(string glue, IEnumerable array)
        {
            var result = new List<string>();

            foreach (var item in array)
            {
                result.Add(item.ToString());
            }

            return string.Join(glue, result);
        }

        public static string ExpandParameters(string sql, string placeholder, object[] bindings)
        {
            return ReplaceAll(sql, placeholder, i =>
            {
                var parameter = bindings[i];

                if (IsArray(parameter))
                {
                    var count = EnumerableCount(parameter as IEnumerable);
                    return string.Join(",", placeholder.Repeat(count));
                }

                return placeholder.ToString();
            });
        }

        public static int EnumerableCount(IEnumerable obj)
        {
            int count = 0;

            foreach (var item in obj)
            {
                count++;
            }

            return count;
        }

        public static List<string> ExpandExpression(string expression)
        {
            var regex = @"^(?:\w+\.){1,2}{(.*)}";
            var match = Regex.Match(expression, regex);

            if (!match.Success)
            {
                // we did not found a match return the string as is.
                return new List<string> { expression };
            }

            var table = expression.Substring(0, expression.IndexOf(".{"));

            var captures = match.Groups[1].Value;

            var cols = Regex.Split(captures, @"\s*,\s*")
                .Select(x => $"{table}.{x.Trim()}")
                .ToList();

            return cols;
        }

        public static IEnumerable<string> Repeat(this string str, int count)
        {
            return Enumerable.Repeat(str, count);
        }

        public static string ReplaceIdentifierUnlessEscaped(this string input, string escapeCharacter, string identifier, string newIdentifier)
        {
            //Replace standard, non-escaped identifiers first
            var nonEscapedRegex = new Regex($@"(?<!{Regex.Escape(escapeCharacter)}){Regex.Escape(identifier)}");
            var nonEscapedReplace = nonEscapedRegex.Replace(input, newIdentifier);

            //Then replace escaped identifiers, by just removing the escape character
            var escapedRegex = new Regex($@"{Regex.Escape(escapeCharacter)}{Regex.Escape(identifier)}");
            return escapedRegex.Replace(nonEscapedReplace, identifier);
        }
    }
}
