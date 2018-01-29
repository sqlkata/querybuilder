using System;

namespace SqlKata.Execution
{
    internal static class QueryHelper
    {
        public static XQuery CastToXQuery(Query query, string method)
        {
            var xQuery = query as XQuery;

            if (xQuery is null)
            {
                throw new InvalidOperationException($"the {method} method can only be used with `XQuery` instances, consider using the `QueryFactory.Query()` to create executable queries, check https://sqlkata.com/docs/execution/setup#xquery-class for more info");
            }

            return xQuery;

        }
    }
}