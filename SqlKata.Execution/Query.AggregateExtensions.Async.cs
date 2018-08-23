using System.Data;
using System.Threading.Tasks;
using Dapper;
using SqlKata;

namespace SqlKata.Execution
{
    public static class QueryAggregateExtensionsAsync
    {
        public static Task<T> AggregateAsync<T>(this Query query, string aggregateOperation, params string[] columns)
        {
            return query.AggregateAsync<T>(aggregateOperation, null, columns);
        }

        public static async Task<T> AggregateAsync<T>(this Query query, string aggregateOperation, IDbTransaction transaction, params string[] columns)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(AggregateAsync), transaction);

            var result = xQuery.Compiler.Compile(query.AsAggregate(aggregateOperation, columns));

            var scalar = await xQuery.Connection.ExecuteScalarAsync<T>(result.Sql, result.NamedBindings, xQuery.Transaction);

            return scalar;

        }

        public static Task<T> CountAsync<T>(this Query query, params string[] columns)
        {
            return query.CountAsync<T>(null, columns);
        }

        public static async Task<T> CountAsync<T>(this Query query, IDbTransaction transaction, params string[] columns)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(CountAsync), transaction);

            var result = xQuery.Compiler.Compile(query.AsCount(columns));

            var scalar = await xQuery.Connection.ExecuteScalarAsync<T>(result.Sql, result.NamedBindings, xQuery.Transaction);

            return scalar;
        }

        public static async Task<T> AverageAsync<T>(this Query query, string column, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(AverageAsync), transaction);
            return await query.AggregateAsync<T>("avg", transaction, column);
        }

        public static async Task<T> SumAsync<T>(this Query query, string column, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(SumAsync), transaction);
            return await query.AggregateAsync<T>("sum", transaction, column);
        }

        public static async Task<T> MinAsync<T>(this Query query, string column, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(MinAsync), transaction);
            return await query.AggregateAsync<T>("min", transaction, column);
        }

        public static async Task<T> MaxAsync<T>(this Query query, string column, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(MaxAsync), transaction);
            return await query.AggregateAsync<T>("max", transaction, column);
        }

    }
}