using System.Threading.Tasks;
using Dapper;
using SqlKata.Execution;

namespace SqlKata.Execution
{
    public static class QueryAggregateExtensions
    {
        public static T Aggregate<T>(this Query query, string aggregateOperation, params string[] columns)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Aggregate));

            var result = xQuery.Compiler.Compile(query.AsAggregate(aggregateOperation, columns));

            var scalar = xQuery.Connection.ExecuteScalar<T>(result.Sql, result.Bindings);

            return scalar;

        }

        public static T Count<T>(this Query query, params string[] columns)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Count));

            var result = xQuery.Compiler.Compile(query.AsCount(columns));

            var scalar = xQuery.Connection.ExecuteScalar<T>(result.Sql, result.Bindings);

            return scalar;
        }

        public static T Average<T>(this Query query, string column)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Average));
            return query.Aggregate<T>("avg", column);
        }

        public static T Sum<T>(this Query query, string column)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Sum));
            return query.Aggregate<T>("sum", column);
        }

        public static T Min<T>(this Query query, string column)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Min));
            return query.Aggregate<T>("min", column);
        }

        public static T Max<T>(this Query query, string column)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Max));
            return query.Aggregate<T>("max", column);
        }

    }
}