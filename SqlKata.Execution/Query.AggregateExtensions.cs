using System.Data;
using SqlKata;

namespace SqlKata.Execution
{
    public static class QueryAggregateExtensions
    {
        public static T Aggregate<T>(this Query query, string aggregateOperation, params string[] columns)
        {
            return query.Aggregate<T>(aggregateOperation, null, columns);
        }

        public static T Aggregate<T>(this Query query, string aggregateOperation, IDbTransaction transaction, params string[] columns)
        {
            var factory = QueryHelper.CreateQueryFactory(query, transaction);

            return factory.ExecuteScalar<T>(query.AsAggregate(aggregateOperation, columns));
        }

        public static T Count<T>(this Query query, params string[] columns)
        {
            return query.Count<T>(null, columns);
        }

        public static T Count<T>(this Query query, IDbTransaction transaction, params string[] columns)
        {
            var factory = QueryHelper.CreateQueryFactory(query, transaction);

            return factory.ExecuteScalar<T>(query.AsCount(columns));
        }

        public static T Average<T>(this Query query, string column, IDbTransaction transaction = null)
        {
            return query.Aggregate<T>("avg", transaction, column);
        }

        public static T Sum<T>(this Query query, string column, IDbTransaction transaction = null)
        {
            return query.Aggregate<T>("sum", transaction, column);
        }

        public static T Min<T>(this Query query, string column, IDbTransaction transaction = null)
        {
            return query.Aggregate<T>("min", transaction, column);
        }

        public static T Max<T>(this Query query, string column, IDbTransaction transaction = null)
        {
            return query.Aggregate<T>("max", transaction, column);
        }

    }
}