using System.Threading.Tasks;

namespace SqlKata.Execution
{
    public static class QueryAggregateExtensionsAsync
    {
        public static async Task<T> AggregateAsync<T>(
            this Query query,
            string aggregateOperation,
            params string[] columns
        )
        {
            var db = QueryHelper.CreateQueryFactory(query);

            return await db.ExecuteScalarAsync<T>(query.AsAggregate(aggregateOperation, columns));
        }

        public static async Task<T> CountAsync<T>(this Query query, params string[] columns)
        {
            var db = QueryHelper.CreateQueryFactory(query);

            return await db.ExecuteScalarAsync<T>(query.AsCount(columns));
        }

        public static async Task<T> AverageAsync<T>(this Query query, string column)
        {
            return await query.AggregateAsync<T>("avg", column);
        }

        public static async Task<T> SumAsync<T>(this Query query, string column)
        {
            return await query.AggregateAsync<T>("sum", column);
        }

        public static async Task<T> MinAsync<T>(this Query query, string column)
        {
            return await query.AggregateAsync<T>("min", column);
        }

        public static async Task<T> MaxAsync<T>(this Query query, string column)
        {
            return await query.AggregateAsync<T>("max", column);
        }

    }
}