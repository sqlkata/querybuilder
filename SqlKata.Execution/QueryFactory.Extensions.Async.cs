using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using System.Threading.Tasks;
using System.Dynamic;
using Humanizer;

namespace SqlKata.Execution
{
    public static class QueryFactoryExtensionsAsync
    {
        #region Dapper

        public static async Task<IEnumerable<T>> GetAsync<T>(this QueryFactory db, Query query)
        {
            var compiled = db.Compile(query);

            var result = (await db.Connection.QueryAsync<T>(
                compiled.Sql,
                compiled.NamedBindings,
                commandTimeout: db.QueryTimeout
            )).ToList();

            result = (await handleIncludesAsync(query, result)).ToList();

            return result;
        }

        public static async Task<IEnumerable<IDictionary<string, object>>> GetDictionaryAsync(this QueryFactory db, Query query)
        {
            var compiled = db.Compile(query);

            var result = await db.Connection.QueryAsync(
                compiled.Sql,
                compiled.NamedBindings,
                commandTimeout: db.QueryTimeout
            );

            return result as IEnumerable<IDictionary<string, object>>;
        }

        public static async Task<IEnumerable<dynamic>> GetAsync(this QueryFactory db, Query query)
        {
            return await GetAsync<dynamic>(db, query);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this QueryFactory db, Query query)
        {
            var list = await GetAsync<T>(db, query.Limit(1));

            return list.ElementAtOrDefault(0);
        }

        public static async Task<dynamic> FirstOrDefaultAsync(this QueryFactory db, Query query)
        {
            return await FirstOrDefaultAsync<dynamic>(db, query);
        }

        public static async Task<T> FirstAsync<T>(this QueryFactory db, Query query)
        {
            var item = await FirstOrDefaultAsync<T>(db, query);

            if (item == null)
            {
                throw new InvalidOperationException("The sequence contains no elements");
            }

            return item;
        }

        public static async Task<dynamic> FirstAsync(this QueryFactory db, Query query)
        {
            return await FirstAsync<dynamic>(db, query);
        }

        public static async Task<int> ExecuteAsync(
            this QueryFactory db,
            Query query,
            IDbTransaction transaction = null,
            CommandType? commandType = null
        )
        {
            var compiled = db.Compile(query);

            return await db.Connection.ExecuteAsync(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                db.QueryTimeout,
                commandType
            );
        }

        public static async Task<T> ExecuteScalarAsync<T>(
            this QueryFactory db,
            Query query,
            IDbTransaction transaction = null,
            CommandType? commandType = null
        )
        {
            var compiled = db.Compile(query.Limit(1));

            return await db.Connection.ExecuteScalarAsync<T>(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                db.QueryTimeout,
                commandType
            );
        }

        public static async Task<SqlMapper.GridReader> GetMultipleAsync<T>(
            this QueryFactory db,
            Query[] queries,
            IDbTransaction transaction = null,
            CommandType? commandType = null
        )
        {
            var compiled = db.Compiler.Compile(queries);

            return await db.Connection.QueryMultipleAsync(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                db.QueryTimeout,
                commandType
            );

        }

        public static async Task<IEnumerable<IEnumerable<T>>> GetAsync<T>(
            this QueryFactory db,
            Query[] queries,
            IDbTransaction transaction = null,
            CommandType? commandType = null
        )
        {

            var multi = await db.GetMultipleAsync<T>(
                queries,
                transaction,
                commandType
            );

            var list = new List<IEnumerable<T>>();

            using (multi)
            {
                for (var i = 0; i < queries.Count(); i++)
                {
                    list.Add(multi.Read<T>());
                }
            }

            return list;

        }

        #endregion

        #region aggregate
        public static async Task<T> AggregateAsync<T>(
            this QueryFactory db,
            Query query,
            string aggregateOperation,
            params string[] columns
        )
        {
            return await db.ExecuteScalarAsync<T>(query.AsAggregate(aggregateOperation, columns));
        }

        public static async Task<T> CountAsync<T>(this QueryFactory db, Query query, params string[] columns)
        {
            return await db.ExecuteScalarAsync<T>(query.AsCount(columns));
        }

        public static async Task<T> AverageAsync<T>(this QueryFactory db, Query query, string column)
        {
            return await db.AggregateAsync<T>(query, "avg", column);
        }

        public static async Task<T> SumAsync<T>(this QueryFactory db, Query query, string column)
        {
            return await db.AggregateAsync<T>(query, "sum", column);
        }

        public static async Task<T> MinAsync<T>(this QueryFactory db, Query query, string column)
        {
            return await db.AggregateAsync<T>(query, "min", column);
        }

        public static async Task<T> MaxAsync<T>(this QueryFactory db, Query query, string column)
        {
            return await db.AggregateAsync<T>(query, "max", column);
        }

        #endregion

        #region pagination
        public static async Task<PaginationResult<T>> PaginateAsync<T>(this QueryFactory db, Query query, int page, int perPage = 25)
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

            IEnumerable<T> list;

            if (count > 0)
            {
                list = await query.Clone().ForPage(page, perPage).GetAsync<T>();
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

        public static async Task ChunkAsync<T>(
            this QueryFactory db,
            Query query,
            int chunkSize,
            Func<IEnumerable<T>, int, bool> func
        )
        {
            var result = await db.PaginateAsync<T>(query, 1, chunkSize);

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

        public static async Task ChunkAsync<T>(
            this QueryFactory db,
            Query query,
            int chunkSize,
            Action<IEnumerable<T>,
            int> action
        )
        {
            var result = await db.PaginateAsync<T>(query, 1, chunkSize);

            action(result.List, 1);

            while (result.HasNext)
            {
                result = result.Next();
                action(result.List, result.Page);
            }

        }
        #endregion

        #region free statements
        public static async Task<IEnumerable<T>> SelectAsync<T>(this QueryFactory db, string sql, object param = null)
        {
            return await db.Connection.QueryAsync<T>(sql, param, commandTimeout: db.QueryTimeout);
        }
        public static async Task<IEnumerable<dynamic>> SelectAsync(this QueryFactory db, string sql, object param = null)
        {
            return await db.SelectAsync<dynamic>(sql, param);
        }
        public static async Task<int> StatementAsync(this QueryFactory db, string sql, object param = null)
        {
            return await db.Connection.ExecuteAsync(sql, param, commandTimeout: db.QueryTimeout);
        }
        #endregion

        // TODO: currently am copying this from the handleInclude (sync) method, refactor this and reuse the common part.
        private static async Task<IEnumerable<T>> handleIncludesAsync<T>(Query query, IEnumerable<T> result)
        {
            if (!result.Any())
            {
                return result;
            }

            bool canBeProcessed = query.Includes.Any() && result.ElementAt(0) is IDynamicMetaObjectProvider;

            if (!canBeProcessed)
            {
                return result;
            }
            List<Dictionary<string, object>> dynamicResult = QueryFactoryHelper.GetDynamicResult(result); //---

            foreach (Include include in query.Includes)
            {
                await processManyIncludeStatements(query, dynamicResult, include);

                if (include.ForeignKey == null)
                {
                    include.ForeignKey = include.Name + "Id";
                }
                List<string> foreignIds = QueryFactoryHelper.GetForeignIDs(dynamicResult, include); //---

                await processManyForeignIDs(dynamicResult, include, foreignIds);
            }

            return dynamicResult.Cast<T>();
        }

        private static async Task processManyForeignIDs(List<Dictionary<string, object>> dynamicResult, Include include, List<string> foreignIds)
        {
            if (foreignIds.Any())
            {
                Dictionary<string, Dictionary<string, object>> related = await getEmbeddedForeignKeysAsync(include, foreignIds);

                foreach (var item in dynamicResult)
                {
                    var foreignValue = item[include.ForeignKey].ToString();
                    item[include.Name] = related.ContainsKey(foreignValue) ? related[foreignValue] : null;
                }
            }
        }

        private static async Task processManyIncludeStatements(Query query, List<Dictionary<string, object>> dynamicResult, Include include)
        {
            if (include.IsMany)
            {
                QueryFactoryHelper.TryFetchingForeignKey(query, include);
                List<string> localIds = QueryFactoryHelper.GetLocalIDs(dynamicResult, include);

                if (localIds.Any())
                {
                    Dictionary<string, List<Dictionary<string, object>>> children = await getEmbeddedIncludesAsync(include, localIds);

                    foreach (var item in dynamicResult)
                    {
                        var localValue = item[include.LocalKey].ToString();
                        item[include.Name] = children.ContainsKey(localValue) ? children[localValue] : new List<Dictionary<string, object>>();
                    }
                }
            }
        }

        private static async Task<Dictionary<string, Dictionary<string, object>>> getEmbeddedForeignKeysAsync(Include include, List<string> foreignIds)
        {
            return (await include.Query.WhereIn(include.LocalKey, foreignIds).GetAsync())
                .Cast<IDictionary<string, object>>()
                .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(x => x[include.LocalKey].ToString());
        }


        private static async Task<Dictionary<string, List<Dictionary<string, object>>>> getEmbeddedIncludesAsync(Include include, List<string> localIds)
        {
            return (await include.Query.WhereIn(include.ForeignKey, localIds).GetAsync())
                .Cast<IDictionary<string, object>>()
                .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
                .GroupBy(x => x[include.ForeignKey].ToString())
                .ToDictionary(x => x.Key, x => x.ToList());
        }
    }
}