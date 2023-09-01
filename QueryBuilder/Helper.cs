using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SqlKata
{
    public static class D
    {
        private sealed class AssertionException : Exception
        {
            internal AssertionException(string msg) : base(msg) { }
        }

        private static void DoAssert(bool b, string str)
        {
            if (!b) throw new AssertionException(str);
        }

        [Conditional("DEBUG")]
        public static void IsTrue(bool b)
        {
            DoAssert(b, "Expression must be true but it is not.");
        }

        [Conditional("DEBUG")]
        public static void IsFalse(bool b, string? message = null)
        {
            DoAssert(!b, message ?? "Expression must be false but it is true.");
        }

      
    }
    public static class Helper
    {
        /// <summary>
        /// Converts "Users.{Id,Name, Last_Name }"
        /// into ["Users.Id", "Users.Name", "Users.Last_Name"]
        /// </summary>
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
