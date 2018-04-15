using Dapper;
using SqlKata.Execution;
using System.Collections.Generic;
using System;

namespace SqlKata.Execution
{
    public static class QueryExtensions
    {
        public static IEnumerable<T> Get<T>(this Query query)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Get));

            var compiled = xQuery.Compiler.Compile(query);

            return xQuery.Connection.Query<T>(compiled.Sql, compiled.Bindings);
        }

        public static IEnumerable<dynamic> Get(this Query query)
        {
            return query.Get<dynamic>();
        }

        public static T FirstOrDefault<T>(this Query query)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(FirstOrDefault));

            var compiled = xQuery.Compiler.Compile(query.Limit(1));

            return xQuery.Connection.QueryFirstOrDefault<T>(compiled.Sql, compiled.Bindings);

        }

        public static dynamic FirstOrDefault(this Query query)
        {
            return FirstOrDefault<dynamic>(query);
        }

        public static T First<T>(this Query query)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(First));

            var compiled = xQuery.Compiler.Compile(query.Limit(1));

            return xQuery.Connection.QueryFirst<T>(compiled.Sql, compiled.Bindings);

        }

        public static dynamic First(this Query query)
        {
            return First<dynamic>(query);
        }

        public static PaginationResult<T> Paginate<T>(this Query query, int page, int perPage = 25)
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

        public static PaginationResult<dynamic> Paginate(this Query query, int page, int perPage = 25)
        {
            return query.Paginate<dynamic>(page, perPage);
        }

        public static void Chunk<T>(this Query query, int chunkSize, Func<IEnumerable<T>, int, bool> func)
        {
            var result = query.Paginate<T>(1, chunkSize);

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

        public static void Chunk(this Query query, int chunkSize, Func<IEnumerable<dynamic>, int, bool> func)
        {
            query.Chunk<dynamic>(chunkSize, func);
        }

        public static void Chunk<T>(this Query query, int chunkSize, Action<IEnumerable<T>, int> action)
        {
            var result = query.Paginate<T>(1, chunkSize);

            action(result.List, 1);

            while (result.HasNext)
            {
                result = result.Next();
                action(result.List, result.Page);
            }

        }

        public static void Chunk(this Query query, int chunkSize, Action<IEnumerable<dynamic>, int> action)
        {
            query.Chunk<dynamic>(chunkSize, action);
        }

        public static int Insert(this Query query, IReadOnlyDictionary<string, object> values)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(values));

            return xQuery.Connection.Execute(compiled.Sql, compiled.Bindings);

        }

        public static int Insert(this Query query, IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(columns, valuesCollection));

            return xQuery.Connection.Execute(compiled.Sql, compiled.Bindings);

        }

        public static int Insert(this Query query, IEnumerable<string> columns, Query fromQuery)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(Insert));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(columns, fromQuery));

            return xQuery.Connection.Execute(compiled.Sql, compiled.Bindings);

        }

        public static int Update(this Query query, IReadOnlyDictionary<string, object> values)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Update));

            var compiled = xQuery.Compiler.Compile(query.AsUpdate(values));

            return xQuery.Connection.Execute(compiled.Sql, compiled.Bindings);
        }

        public static int Delete(this Query query)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(Delete));

            var compiled = xQuery.Compiler.Compile(query.AsDelete());

            return xQuery.Connection.Execute(compiled.Sql, compiled.Bindings);
        }

    }
}