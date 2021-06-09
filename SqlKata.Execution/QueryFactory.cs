using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Humanizer;
using SqlKata.Compilers;

namespace SqlKata.Execution
{
    public class QueryFactory : IDisposable
    {
        public IDbConnection Connection { get; set; }
        public Compiler Compiler { get; set; }
        public Action<SqlResult> Logger = result => { };
        private bool disposedValue;

        public int QueryTimeout { get; set; } = 30;

        public QueryFactory() { }

        public QueryFactory(IDbConnection connection, Compiler compiler, int timeout = 30)
        {
            Connection = connection;
            Compiler = compiler;
            QueryTimeout = timeout;
        }

        public Query Query()
        {
            var query = new XQuery(this.Connection, this.Compiler);

            query.QueryFactory = this;

            query.Logger = Logger;

            return query;
        }

        public Query Query(string table)
        {
            return Query().From(table);
        }

        /// <summary>
        /// Create an XQuery instance from a regular Query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Query FromQuery(Query query)
        {
            var xQuery = new XQuery(this.Connection, this.Compiler);

            xQuery.QueryFactory = this;

            xQuery.Clauses = query.Clauses.Select(x => x.Clone()).ToList();

            xQuery.SetParent(query.Parent);
            xQuery.QueryAlias = query.QueryAlias;
            xQuery.IsDistinct = query.IsDistinct;
            xQuery.Method = query.Method;
            xQuery.Includes = query.Includes;
            xQuery.Variables = query.Variables;

            xQuery.SetEngineScope(query.EngineScope);

            xQuery.Logger = Logger;

            return xQuery;
        }

        public IEnumerable<T> Get<T>(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            var compiled = CompileAndLog(query);

            var result = this.Connection.Query<T>(
                compiled.Sql,
                compiled.NamedBindings,
                transaction: transaction,
                commandTimeout: timeout ?? this.QueryTimeout
            ).ToList();

            result = handleIncludes<T>(query, result).ToList();

            return result;
        }

        public async Task<IEnumerable<T>> GetAsync<T>(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            var compiled = CompileAndLog(query);

            var result = (await this.Connection.QueryAsync<T>(
                compiled.Sql,
                compiled.NamedBindings,
                transaction: transaction,
                commandTimeout: timeout ?? this.QueryTimeout
            )).ToList();

            result = (await handleIncludesAsync(query, result)).ToList();

            return result;
        }

        public IEnumerable<IDictionary<string, object>> GetDictionary(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            var compiled = CompileAndLog(query);

            var result = this.Connection.Query(
                compiled.Sql,
                compiled.NamedBindings,
                transaction: transaction,
                commandTimeout: timeout ?? this.QueryTimeout
            );

            return result.Cast<IDictionary<string, object>>();
        }

        public async Task<IEnumerable<IDictionary<string, object>>> GetDictionaryAsync(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            var compiled = CompileAndLog(query);

            var result = await this.Connection.QueryAsync(
                compiled.Sql,
                compiled.NamedBindings,
                transaction: transaction,
                commandTimeout: timeout ?? this.QueryTimeout
            );

            return result.Cast<IDictionary<string, object>>();
        }

        public IEnumerable<dynamic> Get(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return Get<dynamic>(query, transaction, timeout);
        }

        public async Task<IEnumerable<dynamic>> GetAsync(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return await GetAsync<dynamic>(query, transaction, timeout);
        }

        public T FirstOrDefault<T>(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            var list = Get<T>(query.Limit(1), transaction, timeout);

            return list.ElementAtOrDefault(0);
        }

        public async Task<T> FirstOrDefaultAsync<T>(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            var list = await GetAsync<T>(query.Limit(1), transaction, timeout);

            return list.ElementAtOrDefault(0);
        }

        public dynamic FirstOrDefault(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return FirstOrDefault<dynamic>(query, transaction, timeout);
        }

        public async Task<dynamic> FirstOrDefaultAsync(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return await FirstOrDefaultAsync<dynamic>(query, transaction, timeout);
        }

        public T First<T>(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            var item = FirstOrDefault<T>(query, transaction, timeout);

            if (item == null)
            {
                throw new InvalidOperationException("The sequence contains no elements");
            }

            return item;
        }

        public async Task<T> FirstAsync<T>(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            var item = await FirstOrDefaultAsync<T>(query, transaction, timeout);

            if (item == null)
            {
                throw new InvalidOperationException("The sequence contains no elements");
            }

            return item;
        }

        public dynamic First(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return First<dynamic>(query, transaction, timeout);
        }

        public async Task<dynamic> FirstAsync(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return await FirstAsync<dynamic>(query, transaction, timeout);
        }

        public int Execute(
            Query query,
            IDbTransaction transaction = null,
            int? timeout = null
        )
        {
            var compiled = CompileAndLog(query);

            return this.Connection.Execute(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                timeout ?? this.QueryTimeout
            );
        }

        public async Task<int> ExecuteAsync(
            Query query,
            IDbTransaction transaction = null,
            int? timeout = null
        )
        {
            var compiled = CompileAndLog(query);

            return await this.Connection.ExecuteAsync(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                timeout ?? this.QueryTimeout
            );
        }

        public T ExecuteScalar<T>(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            var compiled = CompileAndLog(query.Limit(1));

            return this.Connection.ExecuteScalar<T>(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                timeout ?? this.QueryTimeout
            );
        }

        public async Task<T> ExecuteScalarAsync<T>(
            Query query,
            IDbTransaction transaction = null,
            int? timeout = null
        )
        {
            var compiled = CompileAndLog(query.Limit(1));

            return await this.Connection.ExecuteScalarAsync<T>(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                timeout ?? this.QueryTimeout
            );
        }

        public SqlMapper.GridReader GetMultiple<T>(
            Query[] queries,
            IDbTransaction transaction = null,
            int? timeout = null
        )
        {
            var compiled = this.Compiler.Compile(queries);

            return this.Connection.QueryMultiple(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                timeout ?? this.QueryTimeout
            );
        }

        public async Task<SqlMapper.GridReader> GetMultipleAsync<T>(
            Query[] queries,
            IDbTransaction transaction = null,
            int? timeout = null)
        {
            var compiled = this.Compiler.Compile(queries);

            return await this.Connection.QueryMultipleAsync(
                compiled.Sql,
                compiled.NamedBindings,
                transaction,
                timeout ?? this.QueryTimeout
            );
        }

        public IEnumerable<IEnumerable<T>> Get<T>(
            Query[] queries,
            IDbTransaction transaction = null,
            int? timeout = null
        )
        {

            var multi = this.GetMultiple<T>(
                queries,
                transaction,
                timeout
            );

            using (multi)
            {
                for (var i = 0; i < queries.Length; i++)
                {
                    yield return multi.Read<T>();
                }
            }
        }

        public async Task<IEnumerable<IEnumerable<T>>> GetAsync<T>(
            Query[] queries,
            IDbTransaction transaction = null,
            int? timeout = null
        )
        {
            var multi = await this.GetMultipleAsync<T>(
                queries,
                transaction,
                timeout
            );

            var list = new List<IEnumerable<T>>();

            using (multi)
            {
                for (var i = 0; i < queries.Length; i++)
                {
                    list.Add(multi.Read<T>());
                }
            }

            return list;
        }

        public bool Exists(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            var clone = query.Clone()
                .ClearComponent("select")
                .SelectRaw("1 as [Exists]")
                .Limit(1);

            var rows = Get(clone, transaction, timeout);

            return rows.Any();
        }

        public async Task<bool> ExistsAsync(Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            var clone = query.Clone()
                .ClearComponent("select")
                .SelectRaw("1 as [Exists]")
                .Limit(1);

            var rows = await GetAsync(clone, transaction, timeout);

            return rows.Any();
        }

        public T Aggregate<T>(
            Query query,
            string aggregateOperation,
            string[] columns = null,
            IDbTransaction transaction = null,
            int? timeout = null
        )
        {
            return this.ExecuteScalar<T>(query.SelectAggregate(aggregateOperation, columns, AggregateColumn.AggregateDistinct.aggregateNonDistinct), transaction, timeout ?? this.QueryTimeout);
        }

        public async Task<T> SelectAggregateAsync<T>(
            Query query,
            string aggregateOperation,
            string[] columns = null,
            IDbTransaction transaction = null,
            int? timeout = null
        )
        {
            return await this.ExecuteScalarAsync<T>(
                query.SelectAggregate(aggregateOperation, columns, AggregateColumn.AggregateDistinct.aggregateNonDistinct),
                transaction,
                timeout
            );
        }

        public T Count<T>(Query query, string[] columns = null, IDbTransaction transaction = null, int? timeout = null)
        {
            return this.ExecuteScalar<T>(
                query.SelectCount(columns),
                transaction,
                timeout
            );
        }

        public async Task<T> SelectCountAsync<T>(Query query, string[] columns = null, IDbTransaction transaction = null, int? timeout = null)
        {
            return await this.ExecuteScalarAsync<T>(query.SelectCount(columns), transaction, timeout);
        }

        public T Average<T>(Query query, string column, IDbTransaction transaction = null, int? timeout = null)
        {
            return this.Aggregate<T>(query, "avg", new[] { column });
        }

        public async Task<T> SelectAverageAsync<T>(Query query, string column)
        {
            return await this.SelectAggregateAsync<T>(query, "avg", new[] { column });
        }

        public T Sum<T>(Query query, string column)
        {
            return this.Aggregate<T>(query, "sum", new[] { column });
        }

        public async Task<T> SelectSumAsync<T>(Query query, string column)
        {
            return await this.SelectAggregateAsync<T>(query, "sum", new[] { column });
        }

        public T Min<T>(Query query, string column)
        {
            return this.Aggregate<T>(query, "min", new[] { column });
        }

        public async Task<T> SelectMinAsync<T>(Query query, string column)
        {
            return await this.SelectAggregateAsync<T>(query, "min", new[] { column });
        }

        public T Max<T>(Query query, string column)
        {
            return this.Aggregate<T>(query, "max", new[] { column });
        }

        public async Task<T> SelectMaxAsync<T>(Query query, string column)
        {
            return await this.SelectAggregateAsync<T>(query, "max", new[] { column });
        }

        public PaginationResult<T> Paginate<T>(Query query, int page, int perPage = 25, IDbTransaction transaction = null, int? timeout = null)
        {
            if (page < 1)
            {
                throw new ArgumentException("Page param should be greater than or equal to 1", nameof(page));
            }

            if (perPage < 1)
            {
                throw new ArgumentException("PerPage param should be greater than or equal to 1", nameof(perPage));
            }

            var count = Count<long>(query.Clone(), null, transaction, timeout);

            IEnumerable<T> list;

            if (count > 0)
            {
                list = Get<T>(query.Clone().ForPage(page, perPage), transaction, timeout);
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

        public async Task<PaginationResult<T>> PaginateAsync<T>(Query query, int page, int perPage = 25, IDbTransaction transaction = null, int? timeout = null)
        {
            if (page < 1)
            {
                throw new ArgumentException("Page param should be greater than or equal to 1", nameof(page));
            }

            if (perPage < 1)
            {
                throw new ArgumentException("PerPage param should be greater than or equal to 1", nameof(perPage));
            }

            var count = await SelectCountAsync<long>(query.Clone(), null, transaction, timeout);

            IEnumerable<T> list;

            if (count > 0)
            {
                list = await GetAsync<T>(query.Clone().ForPage(page, perPage), transaction, timeout);
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

        public void Chunk<T>(
            Query query,
            int chunkSize,
            Func<IEnumerable<T>, int, bool> func,
            IDbTransaction transaction = null,
            int? timeout = null)
        {
            var result = this.Paginate<T>(query, 1, chunkSize, transaction, timeout);

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

        public async Task ChunkAsync<T>(
            Query query,
            int chunkSize,
            Func<IEnumerable<T>, int, bool> func,
            IDbTransaction transaction = null,
            int? timeout = null
        )
        {
            var result = await this.PaginateAsync<T>(query, 1, chunkSize);

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

        public void Chunk<T>(Query query, int chunkSize, Action<IEnumerable<T>, int> action, IDbTransaction transaction = null, int? timeout = null)
        {
            var result = this.Paginate<T>(query, 1, chunkSize, transaction, timeout);

            action(result.List, 1);

            while (result.HasNext)
            {
                result = result.Next();
                action(result.List, result.Page);
            }
        }

        public async Task ChunkAsync<T>(
            Query query,
            int chunkSize,
            Action<IEnumerable<T>, int> action,
            IDbTransaction transaction = null,
            int? timeout = null
        )
        {
            var result = await this.PaginateAsync<T>(query, 1, chunkSize, transaction, timeout);

            action(result.List, 1);

            while (result.HasNext)
            {
                result = result.Next();
                action(result.List, result.Page);
            }
        }

        public IEnumerable<T> Select<T>(string sql, object param = null, IDbTransaction transaction = null, int? timeout = null)
        {
            return this.Connection.Query<T>(
                sql,
                param,
                transaction: transaction,
                commandTimeout: timeout ?? this.QueryTimeout
            );
        }

        public async Task<IEnumerable<T>> SelectAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? timeout = null)
        {
            return await this.Connection.QueryAsync<T>(
                sql,
                param,
                transaction: transaction,
                commandTimeout: timeout ?? this.QueryTimeout
            );
        }

        public IEnumerable<dynamic> Select(string sql, object param = null, IDbTransaction transaction = null, int? timeout = null)
        {
            return this.Select<dynamic>(sql, param, transaction, timeout);
        }

        public async Task<IEnumerable<dynamic>> SelectAsync(string sql, object param = null, IDbTransaction transaction = null, int? timeout = null)
        {
            return await this.SelectAsync<dynamic>(sql, param, transaction, timeout);
        }

        public int Statement(string sql, object param = null, IDbTransaction transaction = null, int? timeout = null)
        {
            return this.Connection.Execute(sql, param, transaction: transaction, commandTimeout: timeout ?? this.QueryTimeout);
        }

        public async Task<int> StatementAsync(string sql, object param = null, IDbTransaction transaction = null, int? timeout = null)
        {
            return await this.Connection.ExecuteAsync(sql, param, transaction: transaction, commandTimeout: timeout ?? this.QueryTimeout);
        }

        private static IEnumerable<T> handleIncludes<T>(Query query, IEnumerable<T> result)
        {
            if (!result.Any())
            {
                return result;
            }

            var canBeProcessed = query.Includes.Any() && result.ElementAt(0) is IDynamicMetaObjectProvider;

            if (!canBeProcessed)
            {
                return result;
            }

            var dynamicResult = result
                .Cast<IDictionary<string, object>>()
                .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (var include in query.Includes)
            {

                if (include.IsMany)
                {
                    if (include.ForeignKey == null)
                    {
                        // try to guess the default key
                        // I will try to fetch the table name if provided and appending the Id as a convention
                        // Here am using Humanizer package to help getting the singular form of the table

                        var fromTable = query.GetOneComponent("from") as FromClause;

                        if (fromTable == null)
                        {
                            throw new InvalidOperationException($"Cannot guess the foreign key for the included relation '{include.Name}'");
                        }

                        var table = fromTable.Alias ?? fromTable.Table;

                        include.ForeignKey = table.Singularize(false) + "Id";
                    }

                    var localIds = dynamicResult.Where(x => x[include.LocalKey] != null)
                    .Select(x => x[include.LocalKey].ToString())
                    .ToList();

                    if (!localIds.Any())
                    {
                        continue;
                    }

                    var children = include
                        .Query
                        .WhereIn(include.ForeignKey, localIds)
                        .Get()
                        .Cast<IDictionary<string, object>>()
                        .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
                        .GroupBy(x => x[include.ForeignKey].ToString())
                        .ToDictionary(x => x.Key, x => x.ToList());

                    foreach (var item in dynamicResult)
                    {
                        var localValue = item[include.LocalKey].ToString();
                        item[include.Name] = children.ContainsKey(localValue) ? children[localValue] : new List<Dictionary<string, object>>();
                    }

                    continue;
                }

                if (include.ForeignKey == null)
                {
                    include.ForeignKey = include.Name + "Id";
                }

                var foreignIds = dynamicResult
                    .Where(x => x[include.ForeignKey] != null)
                    .Select(x => x[include.ForeignKey].ToString())
                    .ToList();

                if (!foreignIds.Any())
                {
                    continue;
                }

                var related = include
                    .Query
                    .WhereIn(include.LocalKey, foreignIds)
                    .Get()
                    .Cast<IDictionary<string, object>>()
                    .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
                    .ToDictionary(x => x[include.LocalKey].ToString());

                foreach (var item in dynamicResult)
                {
                    var foreignValue = item[include.ForeignKey].ToString();
                    item[include.Name] = related.ContainsKey(foreignValue) ? related[foreignValue] : null;
                }
            }

            return dynamicResult.Cast<T>();
        }

        private static async Task<IEnumerable<T>> handleIncludesAsync<T>(Query query, IEnumerable<T> result)
        {
            if (!result.Any())
            {
                return result;
            }

            var canBeProcessed = query.Includes.Any() && result.ElementAt(0) is IDynamicMetaObjectProvider;

            if (!canBeProcessed)
            {
                return result;
            }

            var dynamicResult = result
                .Cast<IDictionary<string, object>>()
                .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (var include in query.Includes)
            {
                if (include.IsMany)
                {
                    if (include.ForeignKey == null)
                    {
                        // try to guess the default key
                        // I will try to fetch the table name if provided and appending the Id as a convention
                        // Here am using Humanizer package to help getting the singular form of the table

                        var fromTable = query.GetOneComponent("from") as FromClause;

                        if (fromTable == null)
                        {
                            throw new InvalidOperationException($"Cannot guess the foreign key for the included relation '{include.Name}'");
                        }

                        var table = fromTable.Alias ?? fromTable.Table;

                        include.ForeignKey = table.Singularize(false) + "Id";
                    }

                    var localIds = dynamicResult.Where(x => x[include.LocalKey] != null)
                    .Select(x => x[include.LocalKey].ToString())
                    .ToList();

                    if (!localIds.Any())
                    {
                        continue;
                    }

                    var children = (await include.Query.WhereIn(include.ForeignKey, localIds).GetAsync())
                        .Cast<IDictionary<string, object>>()
                        .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
                        .GroupBy(x => x[include.ForeignKey].ToString())
                        .ToDictionary(x => x.Key, x => x.ToList());

                    foreach (var item in dynamicResult)
                    {
                        var localValue = item[include.LocalKey].ToString();
                        item[include.Name] = children.ContainsKey(localValue) ? children[localValue] : new List<Dictionary<string, object>>();
                    }

                    continue;
                }

                if (include.ForeignKey == null)
                {
                    include.ForeignKey = include.Name + "Id";
                }

                var foreignIds = dynamicResult.Where(x => x[include.ForeignKey] != null)
                    .Select(x => x[include.ForeignKey].ToString())
                    .ToList();

                if (!foreignIds.Any())
                {
                    continue;
                }

                var related = (await include.Query.WhereIn(include.LocalKey, foreignIds).GetAsync())
                    .Cast<IDictionary<string, object>>()
                    .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
                    .ToDictionary(x => x[include.LocalKey].ToString());

                foreach (var item in dynamicResult)
                {
                    var foreignValue = item[include.ForeignKey].ToString();
                    item[include.Name] = related.ContainsKey(foreignValue) ? related[foreignValue] : null;
                }
            }

            return dynamicResult.Cast<T>();
        }

        /// <summary>
        /// Compile and log query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        internal SqlResult CompileAndLog(Query query)
        {
            var compiled = this.Compiler.Compile(query);

            this.Logger(compiled);

            return compiled;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Connection.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                Connection = null;
                Compiler = null;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~QueryFactory()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
