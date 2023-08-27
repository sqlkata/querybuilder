using System.Collections;

namespace SqlKata.Compilers
{
    public static class StringExt {
        public static string StrJoin(this IEnumerable src, string separator)
        {
            return string.Join(separator, src.Cast<object?>());
        }
        public static string Brace(this string value, string opening, string closing)
        {
            if (value == "*") return value;

            if (string.IsNullOrWhiteSpace(opening) &&
                string.IsNullOrWhiteSpace(closing)) return value;

            return opening + value.Replace(closing, closing + closing) + closing;
        }
    }
}
