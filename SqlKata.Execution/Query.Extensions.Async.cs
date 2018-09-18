using Dapper;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using SqlKata;
using SqlKata.Interfaces;

namespace SqlKata.Execution
{
    public static class QueryExtensionsAsync
    {
        public static async Task<IEnumerable<T>> GetAsync<T>(this IQuery query)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(GetAsync));

            var compiled = xQuery.Compiler.Compile(query);

            xQuery.Logger(compiled);

            return await xQuery.Connection.QueryAsync<T>(compiled.Sql, compiled.NamedBindings);
        }

        public static async Task<IEnumerable<dynamic>> GetAsync(this IQuery query)
        {
            return await query.GetAsync<dynamic>();
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IQuery query)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(FirstOrDefaultAsync));

            var compiled = xQuery.Compiler.Compile(query.Limit(1));

            xQuery.Logger(compiled);

            return await xQuery.Connection.QueryFirstOrDefaultAsync<T>(compiled.Sql, compiled.NamedBindings);

        }

        public static async Task<dynamic> FirstOrDefaultAsync(this IQuery query)
        {
            return await FirstOrDefaultAsync<dynamic>(query);
        }

        public static async Task<T> FirstAsync<T>(this IQuery query)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(FirstAsync));

            var compiled = xQuery.Compiler.Compile(query.Limit(1));

            xQuery.Logger(compiled);

            return await xQuery.Connection.QueryFirstAsync<T>(compiled.Sql, compiled.NamedBindings);

        }

        public static async Task<dynamic> FirstAsync(this IQuery query)
        {
            return await FirstAsync<dynamic>(query);
        }

        public static async Task<PaginationResult<T>> PaginateAsync<T>(this IQuery query, int page, int perPage = 25)
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

            var list = await query.Clone().ForPage(page, perPage).GetAsync<T>();

            return new PaginationResult<T>
            {
                Query = query.Clone(),
                Page = page,
                PerPage = perPage,
                Count = count,
                List = list
            };

        }

        public static async Task<PaginationResult<dynamic>> PaginateAsync(this IQuery query, int page, int perPage = 25)
        {
            return await query.PaginateAsync<dynamic>(page, perPage);
        }

        public static async Task ChunkAsync<T>(this IQuery query, int chunkSize, Func<IEnumerable<T>, int, bool> func)
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

        public static async Task ChunkAsync(this IQuery query, int chunkSize, Func<IEnumerable<dynamic>, int, bool> func)
        {
            await query.ChunkAsync<dynamic>(chunkSize, func);
        }

        public static async Task ChunkAsync<T>(this IQuery query, int chunkSize, Action<IEnumerable<T>, int> action)
        {
            var result = await query.PaginateAsync<T>(1, chunkSize);

            action(result.List, 1);

            while (result.HasNext)
            {
                result = result.Next();
                action(result.List, result.Page);
            }

        }

        public static async Task ChunkAsync(this IQuery query, int chunkSize, Action<IEnumerable<dynamic>, int> action)
        {
            await query.ChunkAsync<dynamic>(chunkSize, action);
        }

        public static async Task<int> InsertAsync(this IQuery query, IReadOnlyDictionary<string, object> values)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(InsertAsync));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(values));

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings);
        }

        public static async Task<int> InsertAsync(this IQuery query, object data)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(InsertAsync));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(data));

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings);
        }

        public static async Task<int> InsertAsync(this IQuery query, IEnumerable<string> columns, IQuery fromQuery)
        {

            var xQuery = QueryHelper.CastToXQuery(query, nameof(InsertAsync));

            var compiled = xQuery.Compiler.Compile(query.AsInsert(columns, fromQuery));

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings);

        }

        public static async Task<int> UpdateAsync(this IQuery query, IReadOnlyDictionary<string, object> values)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(UpdateAsync));

            var compiled = xQuery.Compiler.Compile(query.AsUpdate(values));

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings);
        }

        public static async Task<int> UpdateAsync(this IQuery query, object data)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(UpdateAsync));

            var compiled = xQuery.Compiler.Compile(query.AsUpdate(data));

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings);
        }

        public static async Task<int> DeleteAsync(this IQuery query)
        {
            var xQuery = QueryHelper.CastToXQuery(query, nameof(DeleteAsync));

            var compiled = xQuery.Compiler.Compile(query.AsDelete());

            xQuery.Logger(compiled);

            return await xQuery.Connection.ExecuteAsync(compiled.Sql, compiled.NamedBindings);
        }

    }
}