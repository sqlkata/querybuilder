using Dapper;
using SqlKata.Execution;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace SqlKata.Execution
{
    public static class QueryExtensionsAsync
    {
        public static async Task<IEnumerable<T>> GetAsync<T>(this Query query)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(GetAsync));

            var compiled = xQuery.Compiler.Compile(query);

            return await xQuery.Connection.QueryAsync<T>(compiled.Sql, compiled.Bindings);
        }

        public static async Task<IEnumerable<dynamic>> GetAsync(this Query query)
        {
            return await query.GetAsync<dynamic>();
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Query query)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(FirstOrDefaultAsync));

            var compiled = xQuery.Compiler.Compile(query.Limit(1));

            return await xQuery.Connection.QueryFirstOrDefaultAsync<T>(compiled.Sql, compiled.Bindings);

        }

        public static async Task<dynamic> FirstOrDefaultAsync(this Query query)
        {
            return await FirstOrDefaultAsync<dynamic>(query);
        }

        public static async Task<T> FirstAsync<T>(this Query query)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(FirstAsync));

            var compiled = xQuery.Compiler.Compile(query.Limit(1));

            return await xQuery.Connection.QueryFirstAsync<T>(compiled.Sql, compiled.Bindings);

        }

        public static async Task<dynamic> FirstAsync(this Query query)
        {
            return await FirstAsync<dynamic>(query);
        }

        public static async Task<PaginationResult<T>> PaginateAsync<T>(this Query query, int page, int perPage = 25)
        {

            if (page < 1)
            {
                throw new ArgumentException("Page param should be greater than or equal to 1", nameof(page));
            }

            if (perPage < 1)
            {
                throw new ArgumentException("PerPage param should be greater than or equal to 1", nameof(perPage));
            }

            var xQuery = QueryHelper.CastToXQuery(query, nameof(PaginateAsync));

            var count = await query.Clone().CountAsync<long>();

            var list = await query.ForPage(page, perPage).GetAsync<T>();

            return new PaginationResult<T>
            {
                Query = query.Clone(),
                Page = page,
                PerPage = perPage,
                Count = count,
                List = list
            };

        }

        public static async Task<PaginationResult<dynamic>> PaginateASync(this Query query, int page, int perPage = 25)
        {
            return await query.PaginateAsync<dynamic>(page, perPage);
        }

        public static async Task ChunkAsync<T>(this Query query, int chunkSize, Func<IEnumerable<T>, int, bool> func)
        {
            var result = await query.PaginateAsync<T>(1, chunkSize);

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

        public static async Task ChunkAsync(this Query query, int chunkSize, Func<IEnumerable<dynamic>, int, bool> func)
        {
            await query.ChunkAsync<dynamic>(chunkSize, func);
        }

        public static async Task ChunkAsync<T>(this Query query, int chunkSize, Action<IEnumerable<T>, int> action)
        {
            var result = await query.PaginateAsync<T>(1, chunkSize);

            action(result.List, 1);

            while (result.HasNext)
            {
                result = result.Next();
                action(result.List, result.Page);
            }

        }

        public static async Task ChunkAsync(this Query query, int chunkSize, Action<IEnumerable<dynamic>, int> action)
        {
            await query.ChunkAsync<dynamic>(chunkSize, action);
        }

        public static async Task<int> InsertAsync(this Query query, IReadOnlyDictionary<string, object> values)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(InsertAsync));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(values));

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.Bindings);
        }

        public static async Task<int> UpdateAsync(this Query query, IReadOnlyDictionary<string, object> values)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(UpdateAsync));

            var compiled = xQuery.Compiler.Compile(query.AsUpdate(values));

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.Bindings);
        }

        public static async Task<int> DeleteAsync(this Query query)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(DeleteAsync));

            var compiled = xQuery.Compiler.Compile(query.AsDelete());

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.Bindings);
        }

    }
}