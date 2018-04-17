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
        public static IEnumerable<T> Query<T>(this QueryFactory db, Query query)
        {
            var compiled = db.Compile(query);

            return db.Connection.Query<T>(compiled.Sql, compiled.Bindings);
        }

        public static IEnumerable<dynamic> Query(this QueryFactory db, Query query)
        {
            return Query<dynamic>(db, query);
        }

        public static T First<T>(this QueryFactory db, Query query)
        {
            var compiled = db.Compile(query.Limit(1));

            return db.Connection.QueryFirst<T>(compiled.Sql, compiled.Bindings);
        }

        public static dynamic First(this QueryFactory db, Query query)
        {
            return First<dynamic>(db, query);
        }

        public static T FirstOrDefault<T>(this QueryFactory db, Query query)
        {
            var compiled = db.Compile(query.Limit(1));

            return db.Connection.QueryFirstOrDefault<T>(compiled.Sql, compiled.Bindings);
        }

        public static dynamic FirstOrDefault(this QueryFactory db, Query query)
        {
            return FirstOrDefault<dynamic>(db, query);
        }

        public static int Execute(this QueryFactory db, Query query, IDbTransaction transaction = null, CommandType? commandType = null)
        {
            var compiled = db.Compile(query.Limit(1));

            return db.Connection.Execute(
                compiled.Sql,
                compiled.Bindings,
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
                compiled.Bindings,
                transaction,
                db.QueryTimeout,
                commandType
            );
        }

        public static SqlMapper.GridReader QueryMultiple<T>(
            this QueryFactory db,
            Query[] queries,
            IDbTransaction transaction = null,
            CommandType? commandType = null
        )
        {

            var compiled = queries
                .Select(q => db.Compile(q))
                .Aggregate((a, b) => a + b);

            return db.Connection.QueryMultiple(
                compiled.Sql,
                compiled.Bindings,
                transaction,
                db.QueryTimeout,
                commandType
            );

        }

        public static List<IEnumerable<T>> Query<T>(
            this QueryFactory db,
            Query[] queries,
            IDbTransaction transaction = null,
            CommandType? commandType = null
        )
        {

            var result = new List<IEnumerable<T>>();

            var multi = db.QueryMultiple<T>(
                queries,
                transaction,
                commandType
            );

            using (multi)
            {
                for (var i = 0; i < queries.Count(); i++)
                {
                    result.Add(multi.Read<T>());
                }
            }

            return result;
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
        #endregion

        private static SqlResult Compile(this QueryFactory db, Query query)
        {
            var compiled = db.Compiler.Compile(query);

            db.Logger(compiled);

            return compiled;
        }

    }
}