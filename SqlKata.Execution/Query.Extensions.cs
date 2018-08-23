using Dapper;
using System.Collections.Generic;
using System;
using System.Data;
using SqlKata;

namespace SqlKata.Execution
{
    public static class QueryExtensions
    {
        public static IEnumerable<T> Get<T>(this Query query, IDbTransaction transaction = null)
        {
            return QueryHelper.CreateQueryFactory(query, transaction).Get<T>(query);
        }

        public static IEnumerable<dynamic> Get(this Query query, IDbTransaction transaction = null)
        {
            return query.Get<dynamic>(transaction);
        }

        public static T FirstOrDefault<T>(this Query query, IDbTransaction transaction = null)
        {
            return QueryHelper.CreateQueryFactory(query, transaction).FirstOrDefault<T>(query);
        }

        public static dynamic FirstOrDefault(this Query query, IDbTransaction transaction = null)
        {
            return FirstOrDefault<dynamic>(query, transaction);
        }

        public static T First<T>(this Query query, IDbTransaction transaction = null)
        {
            return QueryHelper.CreateQueryFactory(query, transaction).First<T>(query);
        }

        public static dynamic First(this Query query, IDbTransaction transaction = null)
        {
            return First<dynamic>(query, transaction);
        }

        public static PaginationResult<T> Paginate<T>(this Query query, int page, int perPage = 25, IDbTransaction transaction = null)
        {
            var db = QueryHelper.CreateQueryFactory(query, transaction);

            return db.Paginate<T>(query, page, perPage);
        }

        public static PaginationResult<dynamic> Paginate(this Query query, int page, int perPage = 25, IDbTransaction transaction = null)
        {
            return query.Paginate<dynamic>(page, perPage, transaction);
        }

        public static void Chunk<T>(this Query query, int chunkSize, Func<IEnumerable<T>, int, bool> func, IDbTransaction transaction = null)
        {
            var db = QueryHelper.CreateQueryFactory(query, transaction);

            db.Chunk<T>(query, chunkSize, func);
        }

        public static void Chunk(this Query query, int chunkSize, Func<IEnumerable<dynamic>, int, bool> func, IDbTransaction transaction = null)
        {
            query.Chunk<dynamic>(chunkSize, func, transaction);
        }

        public static void Chunk<T>(this Query query, int chunkSize, Action<IEnumerable<T>, int> action, IDbTransaction transaction = null)
        {
            var db = QueryHelper.CreateQueryFactory(query, transaction);

            db.Chunk(query, chunkSize, action);

        }

        public static void Chunk(this Query query, int chunkSize, Action<IEnumerable<dynamic>, int> action, IDbTransaction transaction = null)
        {
            query.Chunk<dynamic>(chunkSize, action, transaction);
        }

        public static int Insert(this Query query, IReadOnlyDictionary<string, object> values, IDbTransaction transaction = null)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsInsert(values));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);

        }

        public static int Insert(this Query query, IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection, IDbTransaction transaction = null)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsInsert(columns, valuesCollection));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);

        }

        public static int Insert(this Query query, IEnumerable<string> columns, Query fromQuery, IDbTransaction transaction = null)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsInsert(columns, fromQuery));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);

        }

        public static int Insert(this Query query, object data, IDbTransaction transaction = null)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsInsert(data));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);

        }

        public static int Update(this Query query, IReadOnlyDictionary<string, object> values, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Update), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsUpdate(values));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings);
        }

        public static int Update(this Query query, object data, IDbTransaction transaction = null)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Update), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsUpdate(data));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);

        }

        public static int Delete(this Query query, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Delete), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsDelete());

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);
        }

    }
}