using System.Threading.Tasks;
using Dapper;
using SqlKata;

namespace SqlKata.Execution
{
    public static class QueryAggregateExtensionsAsync
    {
        public static async Task<T> AggregateAsync<T>(this Query query, string aggregateOperation, params string[] columns)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(AggregateAsync));

            var result = xQuery.Compiler.Compile(query.AsAggregate(aggregateOperation, columns));

            var scalar = await xQuery.Connection.ExecuteScalarAsync<T>(result.Sql, result.NamedBindings);

            return scalar;

        }

        public static async Task<T> CountAsync<T>(this Query query, params string[] columns)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(CountAsync));

            var result = xQuery.Compiler.Compile(query.AsCount(columns));

            var scalar = await xQuery.Connection.ExecuteScalarAsync<T>(result.Sql, result.NamedBindings);

            return scalar;
        }

        public static async Task<T> AverageAsync<T>(this Query query, string column)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(AverageAsync));
            return await query.AggregateAsync<T>("avg", column);
        }

        public static async Task<T> SumAsync<T>(this Query query, string column)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(SumAsync));
            return await query.AggregateAsync<T>("sum", column);
        }

        public static async Task<T> MinAsync<T>(this Query query, string column)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(MinAsync));
            return await query.AggregateAsync<T>("min", column);
        }

        public static async Task<T> MaxAsync<T>(this Query query, string column)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(MaxAsync));
            return await query.AggregateAsync<T>("max", column);
        }

    }
}