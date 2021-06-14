using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace SqlKata.Tests.Infrastructure
{
    public abstract class TestSupport
    {
        protected readonly TestCompilersContainer Compilers = new TestCompilersContainer();

        /// <summary>
        /// For legacy test support
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected IReadOnlyDictionary<string, string> Compile(Query query)
        {
            return Compilers.Compile(query).ToDictionary(s => s.Key, v => v.Value.ToString());
        }

        /**
         * Tries to simplistically (i.e. without actually parsing) 'reformat'
         * SQL statements so that when writing tests SQL can be formatted across
         * multiple lines for readability.
         */
        private static string StripWhitespace(string value) =>
            new List<(string Pattern, string Replacement)>
                /* language=regex */
                {
                    // Compress all runs of white space (spaces, tabs, new
                    // lines, etc) into a single separating space.
                    (@"\s+", " "),

                    // Strip leading and trailing white space from the whole
                    // expression
                    (@"^ | $", string.Empty),

                    // Remove space after an opening bracket, or surrounding a
                    // closing bracket.
                    (@"(\() | ?(\)) ?", "$1$2"),

                    // Attach commas to the preceding argument
                    (@" ,", ","),
                }
                .Aggregate(
                    value,
                    (x, y) => Regex.Replace(x, y.Pattern, y.Replacement))
            ;

        protected void CheckCompileResult(
            Query query,
            string engine,
            string expected
            )
        {
            Assert.Equal(
                StripWhitespace(expected),
                StripWhitespace(Compilers.CompileFor(engine, query.Clone()).RawSql)
            );
        }
    }
}
