using System.Collections;
using System.Globalization;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public static class BindingExtensions
    {
        private static readonly Type[] NumberTypes =
        {
            typeof(int),
            typeof(long),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(short),
            typeof(ushort),
            typeof(ulong)
        };

        public static string ExpandParameters(string sql, string placeholder, object?[] bindings)
        {
            return ReplaceAll(sql, placeholder, i =>
            {
                if (bindings[i]?.AsArray() is not { } arr) return placeholder;

                var count = arr.Cast<object>().Count();
                return string.Join(",", placeholder.Repeat(count));

            });
        }
        public static string ReplaceAll(string subject, string match, Func<int, string> replace)
        {
            if (string.IsNullOrWhiteSpace(subject) || !subject.Contains(match)) return subject;

            var split = subject.Split(
                new[] { match },
                StringSplitOptions.None
            );

            return split.Skip(1)
                .Select((item, index) => replace(index) + item)
                .Aggregate(new StringBuilder(split.First()), (prev, right) => prev.Append(right))
                .ToString();
        }
        public static string BindArgs(this List<object?> bindings, string rawSql)
        {
            var deepParameters = bindings.FlattenOneLevel().ToList();

            return ReplaceAll(rawSql, "?", i =>
            {
                if (i >= deepParameters.Count)
                    throw new Exception(
                        $"Failed to retrieve a binding at index {i}, the total bindings count is {bindings.Count}");

                return ChangeToSqlValue(deepParameters[i]);
            });


            static string ChangeToSqlValue(object? value)
            {
                if (value == null) return "NULL";

                if (AsArray(value) is {} arr)
                    return arr.StrJoin(",");

                if (NumberTypes.Contains(value.GetType()))
                    return Convert.ToString(value, CultureInfo.InvariantCulture)!;

                if (value is DateTime date)
                {
                    if (date.Date == date) return "'" + date.ToString("yyyy-MM-dd") + "'";

                    return "'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }

                if (value is bool vBool) return vBool ? "true" : "false";

                if (value is Enum vEnum) return Convert.ToInt32(vEnum) + $" /* {vEnum} */";

                // fallback to string
                return "'" + value.ToString()!.Replace("'", "''") + "'";
            }
        }
        public static Dictionary<string, object?> GenerateNamedBindings(this IEnumerable<object?> bindings, string parameterPrefix)
        {
            return bindings.FlattenOneLevel().Select((v, i) => new { i, v })
                .ToDictionary(x => parameterPrefix + x.i, x => x.v);
        }

        private static IEnumerable? AsArray(this object value)
        {
            if (value is string) return null;

            if (value is byte[]) return null;

            return value as IEnumerable;
        }

        /// <summary>
        ///     {1, { 2, 3, {4}}, 5} -> { 1, 2, 3, {4}, 5}
        /// </summary>
        public static IEnumerable<T> FlattenOneLevel<T>(this IEnumerable<T> array)
        {
            foreach (var item in array)
                if (item?.AsArray() is { } arr)
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
    }
}
