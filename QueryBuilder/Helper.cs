using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlKata
{
    public static class Helper
    {
        public static IEnumerable? AsArray(this object value)
        {
            if (value is string) return null;

            if (value is byte[]) return null;

            return value as IEnumerable;
        }
        public static bool IsArray(object? value)
        {
            if (value == null) return false;
            if (value is string) return false;

            if (value is byte[]) return false;

            return value is IEnumerable;
        }

        /// <summary>
        ///     Flat IEnumerable one level down
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(IEnumerable<T> array)
        {
            foreach (var item in array)
                if (item?.AsArray() is {} arr)
                    foreach (var sub in arr)
                    {
                        if (sub == null)
                            throw new InvalidOperationException(
                                "Sub-item cannot be null!");
                        yield return (T)sub;
                    }
                else
                    yield return item;
        }

        public static IEnumerable<object> FlattenDeep(IEnumerable<object> array)
        {
            return array.SelectMany(o => IsArray(o) ? FlattenDeep((IEnumerable<object>)o) : new[] { o });
        }

        public static IEnumerable<int> AllIndexesOf(string str, string value)
        {
            if (string.IsNullOrEmpty(value)) yield break;

            var index = 0;

            do
            {
                index = str.IndexOf(value, index, StringComparison.Ordinal);

                if (index == -1) yield break;

                yield return index;
            } while ((index += value.Length) < str.Length);
        }

        public static string ReplaceAll(string subject, string match, Func<int, string> callback)
        {
            if (string.IsNullOrWhiteSpace(subject) || !subject.Contains(match)) return subject;

            var split = subject.Split(
                new[] { match },
                StringSplitOptions.None
            );

            return split.Skip(1)
                .Select((item, index) => callback(index) + item)
                .Aggregate(new StringBuilder(split.First()), (prev, right) => prev.Append(right))
                .ToString();
        }

        public static string ExpandParameters(string sql, string placeholder, object?[] bindings)
        {
            return ReplaceAll(sql, placeholder, i =>
            {
                if (bindings[i]?.AsArray() is not {} arr) return placeholder;

                var count = arr.Cast<object>().Count();
                return string.Join(",", placeholder.Repeat(count));

            });
        }

        public static List<string> ExpandExpression(string expression)
        {
            var regex = @"^(?:\w+\.){1,2}{(.*)}";
            var match = Regex.Match(expression, regex);

            if (!match.Success)
                // we did not found a match return the string as is.
                return new List<string> { expression };

            var table = expression.Substring(0, expression.IndexOf(".{", StringComparison.Ordinal));

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

        public static string ReplaceIdentifierUnlessEscaped(this string input, string escapeCharacter,
            string identifier, string newIdentifier)
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
