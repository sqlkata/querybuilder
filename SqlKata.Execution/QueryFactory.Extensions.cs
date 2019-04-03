using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;

namespace SqlKata.Execution
{
    public static class QueryFactoryExtensions
    {
        #region Dapper

        public static IEnumerable<T> Get<T>(this QueryFactory db, Query query)
        {
            var compiled = db.Compile(query);

            return db.Connection.Query<T>(
                compiled.Sql,
                compiled.NamedBindings,
                commandTimeout: db.QueryTimeout
            );
        }

        public static IEnumerable<IDictionary<string, object>> GetDictionary(this QueryFactory db, Query query)
        {
            var compiled = db.Compile(query);

            return db.Connection.Query(compiled.Sql, compiled.NamedBindings, commandTimeout: db.QueryTimeout) as IEnumerable<IDictionary<string, object>>;
        }

        public static IEnumerable<dynamic> Get(this QueryFactory db, Query query)
        {
            return Get<dynamic>(db, query);
        }

        public static T First<T>(this QueryFactory db, Query query)
        {
            var compiled = db.Compile(query.Limit(1));

            return db.Connection.QueryFirst<T>(compiled.Sql, compiled.NamedBindings, commandTimeout: db.QueryTimeout);
        }

        public static dynamic First(this QueryFactory db, Query query)
        {
            return First<dynamic>(db, query);
        }

        public static T FirstOrDefault<T>(this QueryFactory db, Query query)
        {
            var compiled = db.Compile(query.Limit(1));

            return db.Connection.QueryFirstOrDefault<T>(compiled.Sql, compiled.NamedBindings, commandTimeout: db.QueryTimeout);
        }

        public static dynamic FirstOrDefault(this QueryFactory db, Query query)
        {
            return FirstOrDefault<dynamic>(db, query);
        }

        public static int Execute(
            this QueryFactory db,
            Query query,
            IDbTransaction transaction = null,
            CommandType? commandType = null
        )
        {
            var compiled = db.Compile(query);

            return db.Connection.Execute(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                db.QueryTimeout,
                commandType
            );
        }

        public static T ExecuteScalar<T>(this QueryFactory db, Query query, IDbTransaction transaction = null, CommandType? commandType = null)
        {
            var compiled = db.Compile(query.Limit(1));

            return db.Connection.ExecuteScalar<T>(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                db.QueryTimeout,
                commandType
            );
        }

        public static SqlMapper.GridReader GetMultiple<T>(
            this QueryFactory db,
            Query[] queries,
            IDbTransaction transaction = null,
            CommandType? commandType = null
        )
        {
            var compiled = db.Compiler.Compile(queries);

            return db.Connection.QueryMultiple(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                db.QueryTimeout,
                commandType
            );

        }

        public static IEnumerable<IEnumerable<T>> Get<T>(
            this QueryFactory db,
            Query[] queries,
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
            this QueryFactory db,
            Query query,
            string aggregateOperation,
            params string[] columns
        )
        {
            return db.ExecuteScalar<T>(query.AsAggregate(aggregateOperation, columns));
        }

        public static T Count<T>(this QueryFactory db, Query query, params string[] columns)
        {
            return db.ExecuteScalar<T>(query.AsCount(columns));
        }

        public static T Average<T>(this QueryFactory db, Query query, string column)
        {
            return db.Aggregate<T>(query, "avg", column);
        }

        public static T Sum<T>(this QueryFactory db, Query query, string column)
        {
            return db.Aggregate<T>(query, "sum", column);
        }

        public static T Min<T>(this QueryFactory db, Query query, string column)
        {
            return db.Aggregate<T>(query, "min", column);
        }

        public static T Max<T>(this QueryFactory db, Query query, string column)
        {
            return db.Aggregate<T>(query, "max", column);
        }

        #endregion

        #region pagination
        public static PaginationResult<T> Paginate<T>(this QueryFactory db, Query query, int page, int perPage = 25)
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

            IEnumerable<T> list;
            if (count > 0)
            {
                list = query.Clone().ForPage(page, perPage).Get<T>();
            }
            else
            {
                list = Enumerable.Empty<T>();
            }

            return new PaginationResult<T>
            {
                Query = query,
                Page = page,
                PerPage = perPage,
                Count = count,
                List = list
            };

        }

        public static void Chunk<T>(this QueryFactory db, Query query, int chunkSize, Func<IEnumerable<T>, int, bool> func)
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

        public static void Chunk<T>(this QueryFactory db, Query query, int chunkSize, Action<IEnumerable<T>, int> action)
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
        public static IEnumerable<T> Select<T>(this QueryFactory db, string sql, object param = null)
        {
            return db.Connection.Query<T>(sql, param, commandTimeout: db.QueryTimeout);
        }
        public static IEnumerable<dynamic> Select(this QueryFactory db, string sql, object param = null)
        {
            return db.Select<dynamic>(sql, param);
        }
        public static int Statement(this QueryFactory db, string sql, object param = null)
        {
            return db.Connection.Execute(sql, param, commandTimeout: db.QueryTimeout);
        }
        #endregion
    }
}