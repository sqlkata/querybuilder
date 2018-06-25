using System;
using SqlKata;

namespace SqlKata.Execution
{
    internal static class QueryHelper
    {
        internal static XQuery CastToXQuery(Query query, string method = null)
        {
            var xQuery = query as XQuery;

            if (!(xQuery is null)) return xQuery;
            if (method == null)
                throw new InvalidOperationException(
                    "Execution methods can only be used with `XQuery` instances, consider using the `QueryFactory.Query()` to create executable queries, check https://sqlkata.com/docs/execution/setup#xquery-class for more info");
            throw new InvalidOperationException(
                $"The method ${method} can only be used with `XQuery` instances, consider using the `QueryFactory.Query()` to create executable queries, check https://sqlkata.com/docs/execution/setup#xquery-class for more info");
        }

        internal static QueryFactory CreateQueryFactory(XQuery xQuery)
        {
            var factory = new QueryFactory(xQuery.Connection, xQuery.Compiler) {Logger = xQuery.Logger};
            return factory;
        }

        internal static QueryFactory CreateQueryFactory(Query query)
        {
            var xQuery = CastToXQuery(query);
            var factory = new QueryFactory(xQuery.Connection, xQuery.Compiler) {Logger = xQuery.Logger};
            return factory;
        }
    }
}