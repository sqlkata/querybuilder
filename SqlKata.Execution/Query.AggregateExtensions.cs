using SqlKata;

namespace SqlKata.Execution
{
    public static class QueryAggregateExtensions
    {
        public static T Aggregate<T>(this Query query, string aggregateOperation, params string[] columns)
        {
            var factory = QueryHelper.CreateQueryFactory(query);

            return factory.ExecuteScalar<T>(query.AsAggregate(aggregateOperation, columns));
        }

        public static T Count<T>(this Query query, params string[] columns)
        {
            var factory = QueryHelper.CreateQueryFactory(query);

            return factory.ExecuteScalar<T>(query.AsCount(columns));
        }

        public static T Average<T>(this Query query, string column)
        {
            return query.Aggregate<T>("avg", column);
        }

        public static T Sum<T>(this Query query, string column)
        {
            return query.Aggregate<T>("sum", column);
        }

        public static T Min<T>(this Query query, string column)
        {
            return query.Aggregate<T>("min", column);
        }

        public static T Max<T>(this Query query, string column)
        {
            return query.Aggregate<T>("max", column);
        }

    }
}