using Dapper;
using System.Collections.Generic;
using System;
using SqlKata;
using SqlKata.Interfaces;

namespace SqlKata.Execution
{
    public static class QueryExtensions
    {
        public static IEnumerable<T> Get<T>(this IQuery query)
        {
            return QueryHelper.CreateQueryFactory(query).Get<T>(query);
        }

        public static IEnumerable<dynamic> Get(this IQuery query)
        {
            return query.Get<dynamic>();
        }

        public static T FirstOrDefault<T>(this IQuery query)
        {
            return QueryHelper.CreateQueryFactory(query).FirstOrDefault<T>(query);
        }

        public static dynamic FirstOrDefault(this IQuery query)
        {
            return FirstOrDefault<dynamic>(query);
        }

        public static T First<T>(this IQuery query)
        {
            return QueryHelper.CreateQueryFactory(query).First<T>(query);
        }

        public static dynamic First(this IQuery query)
        {
            return First<dynamic>(query);
        }

        public static PaginationResult<T> Paginate<T>(this IQuery query, int page, int perPage = 25)
        {
            var db = QueryHelper.CreateQueryFactory(query);

            return db.Paginate<T>(query, page, perPage);
        }

        public static PaginationResult<dynamic> Paginate(this IQuery query, int page, int perPage = 25)
        {
            return query.Paginate<dynamic>(page, perPage);
        }

        public static void Chunk<T>(this IQuery query, int chunkSize, Func<IEnumerable<T>, int, bool> func)
        {
            var db = QueryHelper.CreateQueryFactory(query);

            db.Chunk<T>(query, chunkSize, func);
        }

        public static void Chunk(this IQuery query, int chunkSize, Func<IEnumerable<dynamic>, int, bool> func)
        {
            query.Chunk<dynamic>(chunkSize, func);
        }

        public static void Chunk<T>(this IQuery query, int chunkSize, Action<IEnumerable<T>, int> action)
        {
            var db = QueryHelper.CreateQueryFactory(query);

            db.Chunk(query, chunkSize, action);

        }

        public static void Chunk(this IQuery query, int chunkSize, Action<IEnumerable<dynamic>, int> action)
        {
            query.Chunk<dynamic>(chunkSize, action);
        }

        public static int Insert(this IQuery query, IReadOnlyDictionary<string, object> values)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(values));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings);

        }

        public static int Insert(this IQuery query, IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(columns, valuesCollection));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings);

        }

        public static int Insert(this IQuery query, IEnumerable<string> columns, IQuery fromQuery)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(columns, fromQuery));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings);

        }

        public static int Insert(this IQuery query, object data)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(data));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings);

        }

        public static int Update(this IQuery query, IReadOnlyDictionary<string, object> values)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Update));

            var compiled = xQuery.Compiler.Compile(query.AsUpdate(values));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings);
        }

        public static int Update(this IQuery query, object data)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Update));

            var compiled = xQuery.Compiler.Compile(query.AsUpdate(data));

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings);

        }

        public static int Delete(this IQuery query)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Delete));

            var compiled = xQuery.Compiler.Compile(query.AsDelete());

            xQuery.Logger(compiled);

            return xQuery.Connection.Execute(compiled.Sql, compiled.NamedBindings);
        }

    }
}