using System.Collections.Generic;
using System;

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

            db.Chunk<T>(query, chunkSize, func);
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

        public static int Insert(this Query query, IReadOnlyDictionary<string, object> values, IEnumerable<string> returnColumns = null)
        {
            return QueryHelper.CreateQueryFactory(query).Execute(query.AsInsert(values, returnColumns));
        }

        public static int Insert(
            this Query query,
            IEnumerable<string> columns,
            IEnumerable<IEnumerable<object>> valuesCollection,
            IEnumerable<string> returnColumns = null
        )
        {
            return QueryHelper.CreateQueryFactory(query).Execute(query.AsInsert(columns, valuesCollection, returnColumns));
        }

        public static int Insert(this Query query, IEnumerable<string> columns, Query fromQuery)
        {
            return QueryHelper.CreateQueryFactory(query).Execute(query.AsInsert(columns, fromQuery));
        }

        public static int Insert(this Query query, object data, IEnumerable<string> returnColumns = null)
        {
            return QueryHelper.CreateQueryFactory(query).Execute(query.AsInsert(data, returnColumns));
        }

        public static T InsertGetId<T>(this Query query, object data)
        {
            var db = QueryHelper.CreateQueryFactory(query);

            var row = db.First<InsertGetIdRow<T>>(query.AsInsert(data, true));

            return row.Id;
        }

        public static int Update(this Query query, IReadOnlyDictionary<string, object> values, IEnumerable<string> returnColumns = null)
        {
            return QueryHelper.CreateQueryFactory(query).Execute(query.AsUpdate(values, returnColumns));
        }

        public static int Update(this Query query, object data, IEnumerable<string> returnColumns = null)
        {
            return QueryHelper.CreateQueryFactory(query).Execute(query.AsUpdate(data, returnColumns));
        }

        public static int Delete(this Query query)
        {
            return QueryHelper.CreateQueryFactory(query).Execute(query.AsDelete());
        }

    }
}