using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SqlKata;
using System.Threading.Tasks;
using SqlKata.Execution.Interfaces;
using SqlKata.Interfaces;

namespace SqlKata.Execution
{
    public static class QueryFactoryExtensions
    {
        #region Dapper
        public static IEnumerable<T> Get<T>(this IQueryFactory db, IQuery query)
        {
            var compiled = db.compile(query);

            return db.Connection.Query<T>(compiled.Sql, compiled.NamedBindings);
        }

        public static IEnumerable<IDictionary<string, object>> GetDictionary(this IQueryFactory db, IQuery query)
        {
            var compiled = db.compile(query);

            return db.Connection.Query(compiled.Sql, compiled.NamedBindings) as IEnumerable<IDictionary<string, object>>;
        }

        public static IEnumerable<dynamic> Get(this IQueryFactory db, IQuery query)
        {
            return Get<dynamic>(db, query);
        }

        public static T First<T>(this IQueryFactory db, IQuery query)
        {
            var compiled = db.compile(query.Limit(1));

            return db.Connection.QueryFirst<T>(compiled.Sql, compiled.NamedBindings);
        }

        public static dynamic First(this IQueryFactory db, IQuery query)
        {
            return First<dynamic>(db, query);
        }

        public static T FirstOrDefault<T>(this IQueryFactory db, IQuery query)
        {
            var compiled = db.compile(query.Limit(1));

            return db.Connection.QueryFirstOrDefault<T>(compiled.Sql, compiled.NamedBindings);
        }

        public static dynamic FirstOrDefault(this IQueryFactory db, IQuery query)
        {
            return FirstOrDefault<dynamic>(db, query);
        }

        public static int Execute(this IQueryFactory db, IQuery query, IDbTransaction transaction = null, CommandType? commandType = null)
        {
            var compiled = db.compile(query);

            return db.Connection.Execute(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                db.QueryTimeout,
                commandType
            );
        }

        public static T ExecuteScalar<T>(this IQueryFactory db, IQuery query, IDbTransaction transaction = null, CommandType? commandType = null)
        {
            var compiled = db.compile(query.Limit(1));

            return db.Connection.ExecuteScalar<T>(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                db.QueryTimeout,
                commandType
            );
        }

        public static SqlMapper.GridReader GetMultiple<T>(
            this IQueryFactory db,
            IQuery[] queries,
            IDbTransaction transaction = null,
            CommandType? commandType = null
        )
        {

            var compiled = queries
                .Select(q => db.compile(q))
                .Aggregate((a, b) => a + b);

            return db.Connection.QueryMultiple(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                db.QueryTimeout,
                commandType
            );

        }

        public static IEnumerable<IEnumerable<T>> Get<T>(
            this IQueryFactory db,
            IQuery[] queries,
            IDbTransaction transaction = null,
            CommandType? commandType = null
        )
        {

            var multi = db.GetMultiple<T>(
                queries,
                transaction,
                commandType
            );

            using (multi)
            {
                for (var i = 0; i < queries.Count(); i++)
                {
                    yield return multi.Read<T>();
                }
            }

        }

        #endregion

        #region aggregate
        public static T Aggregate<T>(
            this IQueryFactory db,
            IQuery query,
            string aggregateOperation,
            params string[] columns
        )
        {
            return db.ExecuteScalar<T>(query.AsAggregate(aggregateOperation, columns));
        }

        public static T Count<T>(this IQueryFactory db, IQuery query, params string[] columns)
        {
            return db.ExecuteScalar<T>(query.AsCount(columns));
        }

        public static T Average<T>(this IQueryFactory db, IQuery query, string column)
        {
            return db.Aggregate<T>(query, "avg", column);
        }

        public static T Sum<T>(this IQueryFactory db, IQuery query, string column)
        {
            return db.Aggregate<T>(query, "sum", column);
        }

        public static T Min<T>(this IQueryFactory db, IQuery query, string column)
        {
            return db.Aggregate<T>(query, "min", column);
        }

        public static T Max<T>(this IQueryFactory db, IQuery query, string column)
        {
            return db.Aggregate<T>(query, "max", column);
        }

        #endregion

        #region pagination
        public static PaginationResult<T> Paginate<T>(this IQueryFactory db, IQuery query, int page, int perPage = 25)
        {

            if (page < 1)
            {
                throw new ArgumentException("Page param should be greater than or equal to 1", nameof(page));
            }

            if (perPage < 1)
            {
                throw new ArgumentException("PerPage param should be greater than or equal to 1", nameof(perPage));
            }

            var count = query.Clone().Count<long>();

            var list = query.Clone().ForPage(page, perPage).Get<T>();

            return new PaginationResult<T>
            {
                Query = query,
                Page = page,
                PerPage = perPage,
                Count = count,
                List = list
            };

        }

        public static void Chunk<T>(this IQueryFactory db, IQuery query, int chunkSize, Func<IEnumerable<T>, int, bool> func)
        {
            var result = db.Paginate<T>(query, 1, chunkSize);

            if (!func(result.List, 1))
            {
                return;
            }

            while (result.HasNext)
            {
                result = result.Next();
                if (!func(result.List, result.Page))
                {
                    return;
                }
            }

        }

        public static void Chunk<T>(this IQueryFactory db, IQuery query, int chunkSize, Action<IEnumerable<T>, int> action)
        {
            var result = db.Paginate<T>(query, 1, chunkSize);

            action(result.List, 1);

            while (result.HasNext)
            {
                result = result.Next();
                action(result.List, result.Page);
            }

        }
        #endregion

        #region free statements
        public static IEnumerable<T> Select<T>(this IQueryFactory db, string sql, object param = null)
        {
            return db.Connection.Query<T>(sql, param);
        }
        public static IEnumerable<dynamic> Select(this IQueryFactory db, string sql, object param = null)
        {
            return db.Select<dynamic>(sql, param);
        }
        public static int Statement(this IQueryFactory db, string sql, object param = null)
        {
            return db.Connection.Execute(sql, param);
        }

        public static async Task<IEnumerable<T>> SelectAsync<T>(this IQueryFactory db, string sql, object param = null)
        {
            return await db.Connection.QueryAsync<T>(sql, param);
        }
        public static async Task<IEnumerable<dynamic>> SelectAsync(this IQueryFactory db, string sql, object param = null)
        {
            return await db.SelectAsync<dynamic>(sql, param);
        }
        public static async Task<int> StatementAsync(this IQueryFactory db, string sql, object param = null)
        {
            return await db.Connection.ExecuteAsync(sql, param);
        }
        #endregion

        private static SqlResult compile(this IQueryFactory db, IQuery query)
        {
            var compiled = db.Compiler.Compile(query);

            db.Logger(compiled);

            return compiled;
        }

    }
}