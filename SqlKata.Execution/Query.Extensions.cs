using System;
using System.Collections.Generic;
using Dapper;

namespace SqlKata.Execution
{
    public static class QueryExtensions
    {
        public static IEnumerable<T> Get<T>(this Query query)
        {
            return QueryHelper.CreateQueryFactory(query).Get<T>(query);
        }

        public static IEnumerable<dynamic> Get(this Query query)
        {
            return query.Get<dynamic>();
        }

        public static T FirstOrDefault<T>(this Query query)
        {
            return QueryHelper.CreateQueryFactory(query).FirstOrDefault<T>(query);
        }

        public static dynamic FirstOrDefault(this Query query)
        {
            return FirstOrDefault<dynamic>(query);
        }

        public static T First<T>(this Query query)
        {
            return QueryHelper.CreateQueryFactory(query).First<T>(query);
        }

        public static dynamic First(this Query query)
        {
            return First<dynamic>(query);
        }

        public static PaginationResult<T> Paginate<T>(this Query query, int page, int perPage = 25)
        {
            var db = QueryHelper.CreateQueryFactory(query);

            return db.Paginate<T>(query, page, perPage);
        }

        public static PaginationResult<dynamic> Paginate(this Query query, int page, int perPage = 25)
        {
            return query.Paginate<dynamic>(page, perPage);
        }

        public static void Chunk<T>(this Query query, int chunkSize, Func<IEnumerable<T>, int, bool> func)
        {
            var db = QueryHelper.CreateQueryFactory(query);

            db.Chunk(query, chunkSize, func);
        }

        public static void Chunk(this Query query, int chunkSize, Func<IEnumerable<dynamic>, int, bool> func)
        {
            query.Chunk<dynamic>(chunkSize, func);
        }

        public static void Chunk<T>(this Query query, int chunkSize, Action<IEnumerable<T>, int> action)
        {
            var db = QueryHelper.CreateQueryFactory(query);

            db.Chunk(query, chunkSize, action);
        }

        public static void Chunk(this Query query, int chunkSize, Action<IEnumerable<dynamic>, int> action)
        {
            query.Chunk<dynamic>(chunkSize, action);
        }

        public static int Insert(this Query query, IReadOnlyDictionary<string, object> values)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(values));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.Bindings);
        }

        public static int Insert(this Query query, IEnumerable<string> columns,
            IEnumerable<IEnumerable<object>> valuesCollection)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(columns, valuesCollection));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.Bindings);
        }

        public static int Insert(this Query query, IEnumerable<string> columns, Query fromQuery)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(columns, fromQuery));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.Bindings);
        }

        public static int Update(this Query query, IReadOnlyDictionary<string, object> values)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Update));

            var compiled = xQuery.Compiler.Compile(query.AsUpdate(values));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.Bindings);
        }

        public static int Delete(this Query query)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Delete));

            var compiled = xQuery.Compiler.Compile(query.AsDelete());

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.Bindings);
        }
    }
}