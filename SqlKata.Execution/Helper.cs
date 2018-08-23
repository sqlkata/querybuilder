using System;
using System.Data;
using SqlKata;

namespace SqlKata.Execution
{
    internal static class QueryHelper
    {
        internal static XQuery CastToXQuery(Query query, string method = null, IDbTransaction transaction = null)
        {
            var xQuery = query as XQuery;

            if (xQuery is null)
            {
                if (method == null)
                {
                    throw new InvalidOperationException($"Execution methods can only be used with `XQuery` instances, consider using the `QueryFactory.Query()` to create executable queries, check https://sqlkata.com/docs/execution/setup#xquery-class for more info");
                }
                else
                {
                    throw new InvalidOperationException($"The method ${method} can only be used with `XQuery` instances, consider using the `QueryFactory.Query()` to create executable queries, check https://sqlkata.com/docs/execution/setup#xquery-class for more info");
                }
            }

            return xQuery;

        }

        internal static QueryFactory CreateQueryFactory(XQuery xQuery)
        {
            var factory = new QueryFactory(xQuery.Connection, xQuery.Compiler, xQuery.Transaction);

            factory.Logger = xQuery.Logger;

            return factory;
        }

        internal static QueryFactory CreateQueryFactory(Query query, IDbTransaction transaction = null)
        {
            var xQuery = CastToXQuery(query);

            var factory = new QueryFactory(xQuery.Connection, xQuery.Compiler, transaction);

            factory.Logger = xQuery.Logger;

            return factory;
        }
    }
}