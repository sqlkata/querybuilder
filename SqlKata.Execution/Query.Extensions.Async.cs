using Dapper;
using System.Collections.Generic;
using System;
using System.Data;
using System.Threading.Tasks;
using SqlKata;

namespace SqlKata.Execution
{
    public static class QueryExtensionsAsync
    {
        public static async Task<IEnumerable<T>> GetAsync<T>(this Query query, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(GetAsync), transaction);

            var compiled = xQuery.Compiler.Compile(query);

            xQuery.Logger(compiled);

            return await xQuery.Connection.QueryAsync<T>(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);
        }

        public static async Task<IEnumerable<dynamic>> GetAsync(this Query query, IDbTransaction transaction = null)
        {
            return await query.GetAsync<dynamic>(transaction);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Query query, IDbTransaction transaction = null)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(FirstOrDefaultAsync), transaction);

            var compiled = xQuery.Compiler.Compile(query.Limit(1));

            xQuery.Logger(compiled);

            return await xQuery.Connection.QueryFirstOrDefaultAsync<T>(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);

        }

        public static async Task<dynamic> FirstOrDefaultAsync(this Query query, IDbTransaction transaction = null)
        {
            return await FirstOrDefaultAsync<dynamic>(query, transaction);
        }

        public static async Task<T> FirstAsync<T>(this Query query, IDbTransaction transaction = null)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(FirstAsync), transaction);

            var compiled = xQuery.Compiler.Compile(query.Limit(1));

            xQuery.Logger(compiled);

            return await xQuery.Connection.QueryFirstAsync<T>(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);

        }

        public static async Task<dynamic> FirstAsync(this Query query, IDbTransaction transaction = null)
        {
            return await FirstAsync<dynamic>(query, transaction);
        }

        public static async Task<PaginationResult<T>> PaginateAsync<T>(this Query query, int page, int perPage = 25, IDbTransaction transaction = null)
        {

            if (page < 1)
            {
                throw new ArgumentException("Page param should be greater than or equal to 1", nameof(page));
            }

            if (perPage < 1)
            {
                throw new ArgumentException("PerPage param should be greater than or equal to 1", nameof(perPage));
            }

            var count = await query.Clone().CountAsync<long>();

            var list = await query.Clone().ForPage(page, perPage).GetAsync<T>(transaction);

            return new PaginationResult<T>
            {
                Query = query.Clone(),
                Page = page,
                PerPage = perPage,
                Count = count,
                List = list
            };

        }

        public static async Task<PaginationResult<dynamic>> PaginateAsync(this Query query, int page, int perPage = 25, IDbTransaction transaction = null)
        {
            return await query.PaginateAsync<dynamic>(page, perPage, transaction);
        }

        public static async Task ChunkAsync<T>(this Query query, int chunkSize, Func<IEnumerable<T>, int, bool> func, IDbTransaction transaction = null)
        {
            var result = await query.PaginateAsync<T>(1, chunkSize, transaction);

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

        public static async Task ChunkAsync(this Query query, int chunkSize, Func<IEnumerable<dynamic>, int, bool> func, IDbTransaction transaction = null)
        {
            await query.ChunkAsync<dynamic>(chunkSize, func, transaction);
        }

        public static async Task ChunkAsync<T>(this Query query, int chunkSize, Action<IEnumerable<T>, int> action, IDbTransaction transaction = null)
        {
            var result = await query.PaginateAsync<T>(1, chunkSize, transaction);

            action(result.List, 1);

            while (result.HasNext)
            {
                result = result.Next();
                action(result.List, result.Page);
            }

        }

        public static async Task ChunkAsync(this Query query, int chunkSize, Action<IEnumerable<dynamic>, int> action, IDbTransaction transaction = null)
        {
            await query.ChunkAsync<dynamic>(chunkSize, action, transaction);
        }

        public static async Task<int> InsertAsync(this Query query, IReadOnlyDictionary<string, object> values, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(InsertAsync), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsInsert(values));

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);
        }

        public static async Task<int> InsertAsync(this Query query, object data, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(InsertAsync), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsInsert(data));

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);
        }

        public static async Task<int> InsertAsync(this Query query, IEnumerable<string> columns, Query fromQuery, IDbTransaction transaction = null)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(InsertAsync), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsInsert(columns, fromQuery));

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);

        }

        public static async Task<int> UpdateAsync(this Query query, IReadOnlyDictionary<string, object> values, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(UpdateAsync), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsUpdate(values));

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);
        }

        public static async Task<int> UpdateAsync(this Query query, object data, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(UpdateAsync), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsUpdate(data));

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);
        }

        public static async Task<int> DeleteAsync(this Query query, IDbTransaction transaction = null)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(DeleteAsync), transaction);

            var compiled = xQuery.Compiler.Compile(query.AsDelete());

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings, xQuery.Transaction);
        }

    }
}