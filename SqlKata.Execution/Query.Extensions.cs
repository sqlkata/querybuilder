using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Data;
using System.Threading;

namespace SqlKata.Execution
{
    /// <summary>
    /// Provides convenience extension methods for executing executable <see cref="Query"/> instances.
    /// </summary>
    public static class QueryExtensions
    {
        /// <summary>
        /// Executes the query and returns whether it yields at least one row.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns><see langword="true"/> when the query returns at least one row; otherwise, <see langword="false"/>.</returns>
        public static bool Exists(this Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).Exists(query, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query and returns whether it yields at least one row.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is <see langword="true"/> when the query returns at least one row; otherwise, <see langword="false"/>.</returns>
        public async static Task<bool> ExistsAsync(this Query query, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).ExistsAsync(query, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query and returns whether it yields no rows.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns><see langword="true"/> when the query returns no rows; otherwise, <see langword="false"/>.</returns>
        public static bool NotExist(this Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return !CreateQueryFactory(query).Exists(query, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query and returns whether it yields no rows.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is <see langword="true"/> when the query returns no rows; otherwise, <see langword="false"/>.</returns>
        public async static Task<bool> NotExistAsync(this Query query, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return !(await CreateQueryFactory(query).ExistsAsync(query, transaction, timeout, cancellationToken));
        }

        /// <summary>
        /// Executes the query and maps all result rows to the requested type.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The sequence of materialized rows.</returns>
        public static IEnumerable<T> Get<T>(this Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).Get<T>(query, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query and maps all result rows to the requested type.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result contains the materialized rows.</returns>
        public static async Task<IEnumerable<T>> GetAsync<T>(this Query query, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).GetAsync<T>(query, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query and returns all rows as dynamic objects.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The sequence of dynamic rows.</returns>
        public static IEnumerable<dynamic> Get(this Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return query.Get<dynamic>(transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query and returns all rows as dynamic objects.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result contains the dynamic rows.</returns>
        public static async Task<IEnumerable<dynamic>> GetAsync(this Query query, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await GetAsync<dynamic>(query, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query and returns the first row mapped to the requested type, or the default value when no row exists.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The first materialized row, or the default value for <typeparamref name="T"/> when no row exists.</returns>
        public static T FirstOrDefault<T>(this Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).FirstOrDefault<T>(query, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query and returns the first row mapped to the requested type, or the default value when no row exists.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the first materialized row, or the default value for <typeparamref name="T"/> when no row exists.</returns>
        public static async Task<T> FirstOrDefaultAsync<T>(this Query query, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).FirstOrDefaultAsync<T>(query, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query and returns the first row as a dynamic object, or <see langword="null"/> when no row exists.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The first dynamic row, or <see langword="null"/> when no row exists.</returns>
        public static dynamic FirstOrDefault(this Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return FirstOrDefault<dynamic>(query, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query and returns the first row as a dynamic object, or <see langword="null"/> when no row exists.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the first dynamic row, or <see langword="null"/> when no row exists.</returns>
        public static async Task<dynamic> FirstOrDefaultAsync(this Query query, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await FirstOrDefaultAsync<dynamic>(query, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query and returns the first row mapped to the requested type.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The first materialized row.</returns>
        public static T First<T>(this Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).First<T>(query, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query and returns the first row mapped to the requested type.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the first materialized row.</returns>
        public static async Task<T> FirstAsync<T>(this Query query, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).FirstAsync<T>(query, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query and returns the first row as a dynamic object.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The first dynamic row.</returns>
        public static dynamic First(this Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return First<dynamic>(query, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query and returns the first row as a dynamic object.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the first dynamic row.</returns>
        public static async Task<dynamic> FirstAsync(this Query query, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await FirstAsync<dynamic>(query, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query and returns a page of rows mapped to the requested type.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="page">The one-based page number to retrieve.</param>
        /// <param name="perPage">The number of rows to include per page.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The requested pagination result.</returns>
        public static PaginationResult<T> Paginate<T>(this Query query, int page, int perPage = 25, IDbTransaction transaction = null, int? timeout = null)
        {
            var db = CreateQueryFactory(query);

            return db.Paginate<T>(query, page, perPage, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query and returns a page of rows mapped to the requested type.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="page">The one-based page number to retrieve.</param>
        /// <param name="perPage">The number of rows to include per page.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the requested pagination result.</returns>
        public static async Task<PaginationResult<T>> PaginateAsync<T>(this Query query, int page, int perPage = 25, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            var db = CreateQueryFactory(query);

            return await db.PaginateAsync<T>(query, page, perPage, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query and returns a page of dynamic rows.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="page">The one-based page number to retrieve.</param>
        /// <param name="perPage">The number of rows to include per page.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The requested pagination result.</returns>
        public static PaginationResult<dynamic> Paginate(this Query query, int page, int perPage = 25, IDbTransaction transaction = null, int? timeout = null)
        {
            return query.Paginate<dynamic>(page, perPage, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query and returns a page of dynamic rows.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="page">The one-based page number to retrieve.</param>
        /// <param name="perPage">The number of rows to include per page.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the requested pagination result.</returns>
        public static async Task<PaginationResult<dynamic>> PaginateAsync(this Query query, int page, int perPage = 25, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await PaginateAsync<dynamic>(query, page, perPage, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query in chunks and stops when the callback returns <see langword="false"/>.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="chunkSize">The maximum number of rows to read per chunk.</param>
        /// <param name="func">The callback that receives each chunk and its one-based page number.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        public static void Chunk<T>(this Query query, int chunkSize, Func<IEnumerable<T>, int, bool> func, IDbTransaction transaction = null, int? timeout = null)
        {
            var db = CreateQueryFactory(query);

            db.Chunk<T>(query, chunkSize, func, transaction, timeout);
        }
        /// <summary>
        /// Asynchronously executes the query in chunks and stops when the callback returns <see langword="false"/>.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="chunkSize">The maximum number of rows to read per chunk.</param>
        /// <param name="func">The callback that receives each chunk and its one-based page number.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        public static async Task ChunkAsync<T>(this Query query, int chunkSize, Func<IEnumerable<T>, int, bool> func, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            await CreateQueryFactory(query).ChunkAsync<T>(query, chunkSize, func, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query in dynamic chunks and stops when the callback returns <see langword="false"/>.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="chunkSize">The maximum number of rows to read per chunk.</param>
        /// <param name="func">The callback that receives each dynamic chunk and its one-based page number.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        public static void Chunk(this Query query, int chunkSize, Func<IEnumerable<dynamic>, int, bool> func, IDbTransaction transaction = null, int? timeout = null)
        {
            query.Chunk<dynamic>(chunkSize, func, transaction, timeout);
        }
        /// <summary>
        /// Asynchronously executes the query in dynamic chunks and stops when the callback returns <see langword="false"/>.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="chunkSize">The maximum number of rows to read per chunk.</param>
        /// <param name="func">The callback that receives each dynamic chunk and its one-based page number.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        public static async Task ChunkAsync(this Query query, int chunkSize, Func<IEnumerable<dynamic>, int, bool> func, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            await ChunkAsync<dynamic>(query, chunkSize, func, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query in chunks and invokes an action for each chunk.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="chunkSize">The maximum number of rows to read per chunk.</param>
        /// <param name="action">The action that receives each chunk and its one-based page number.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        public static void Chunk<T>(this Query query, int chunkSize, Action<IEnumerable<T>, int> action, IDbTransaction transaction = null, int? timeout = null)
        {
            var db = CreateQueryFactory(query);

            db.Chunk(query, chunkSize, action, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query in chunks and invokes an action for each chunk.
        /// </summary>
        /// <typeparam name="T">The row type to materialize.</typeparam>
        /// <param name="query">The executable query to run.</param>
        /// <param name="chunkSize">The maximum number of rows to read per chunk.</param>
        /// <param name="action">The action that receives each chunk and its one-based page number.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        public static async Task ChunkAsync<T>(this Query query, int chunkSize, Action<IEnumerable<T>, int> action, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            await CreateQueryFactory(query).ChunkAsync<T>(query, chunkSize, action, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes the query in dynamic chunks and invokes an action for each chunk.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="chunkSize">The maximum number of rows to read per chunk.</param>
        /// <param name="action">The action that receives each dynamic chunk and its one-based page number.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        public static void Chunk(this Query query, int chunkSize, Action<IEnumerable<dynamic>, int> action, IDbTransaction transaction = null, int? timeout = null)
        {
            query.Chunk<dynamic>(chunkSize, action, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query in dynamic chunks and invokes an action for each chunk.
        /// </summary>
        /// <param name="query">The executable query to run.</param>
        /// <param name="chunkSize">The maximum number of rows to read per chunk.</param>
        /// <param name="action">The action that receives each dynamic chunk and its one-based page number.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        public static async Task ChunkAsync(this Query query, int chunkSize, Action<IEnumerable<dynamic>, int> action, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            await ChunkAsync<dynamic>(query, chunkSize, action, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes an insert using key-value column data.
        /// </summary>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="values">The column names and values to insert.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The number of affected rows.</returns>
        public static int Insert(this Query query, IEnumerable<KeyValuePair<string, object>> values, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).Execute(query.AsInsert(values), transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes an insert using key-value column data.
        /// </summary>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="values">The column names and values to insert.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the number of affected rows.</returns>
        public static async Task<int> InsertAsync(this Query query, IEnumerable<KeyValuePair<string, object>> values, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).ExecuteAsync(query.AsInsert(values), transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes a multi-row insert using explicit columns and value rows.
        /// </summary>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="columns">The columns to insert.</param>
        /// <param name="valuesCollection">The row values to insert for the specified columns.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The number of affected rows.</returns>
        public static int Insert(this Query query, IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).Execute(query.AsInsert(columns, valuesCollection), transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes a multi-row insert using explicit columns and value rows.
        /// </summary>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="columns">The columns to insert.</param>
        /// <param name="valuesCollection">The row values to insert for the specified columns.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the number of affected rows.</returns>
        public static async Task<int> InsertAsync(this Query query, IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).ExecuteAsync(query.AsInsert(columns, valuesCollection), transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes an insert that takes its values from another query.
        /// </summary>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="columns">The target columns to insert into.</param>
        /// <param name="fromQuery">The source query that provides inserted values.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The number of affected rows.</returns>
        public static int Insert(this Query query, IEnumerable<string> columns, Query fromQuery, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).Execute(query.AsInsert(columns, fromQuery), transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes an insert that takes its values from another query.
        /// </summary>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="columns">The target columns to insert into.</param>
        /// <param name="fromQuery">The source query that provides inserted values.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the number of affected rows.</returns>
        public static async Task<int> InsertAsync(this Query query, IEnumerable<string> columns, Query fromQuery, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).ExecuteAsync(query.AsInsert(columns, fromQuery), transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes an insert using public properties from an object as column data.
        /// </summary>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="data">The object whose public properties supply inserted column values.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The number of affected rows.</returns>
        public static int Insert(this Query query, object data, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).Execute(query.AsInsert(data), transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes an insert using public properties from an object as column data.
        /// </summary>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="data">The object whose public properties supply inserted column values.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the number of affected rows.</returns>
        public static async Task<int> InsertAsync(this Query query, object data, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).ExecuteAsync(query.AsInsert(data), transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes an insert and returns the generated identifier.
        /// </summary>
        /// <typeparam name="T">The identifier type to return.</typeparam>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="data">The object whose public properties supply inserted column values.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The generated identifier.</returns>
        public static T InsertGetId<T>(this Query query, object data, IDbTransaction transaction = null, int? timeout = null)
        {
            var db = CreateQueryFactory(query);

            var row = db.First<InsertGetIdRow<T>>(query.AsInsert(data, true), transaction, timeout);

            return row.Id;
        }

        /// <summary>
        /// Asynchronously executes an insert and returns the generated identifier.
        /// </summary>
        /// <typeparam name="T">The identifier type to return.</typeparam>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="data">The object whose public properties supply inserted column values.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the generated identifier.</returns>
        public static async Task<T> InsertGetIdAsync<T>(this Query query, object data, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            var row = await CreateQueryFactory(query)
                .FirstAsync<InsertGetIdRow<T>>(query.AsInsert(data, true), transaction, timeout, cancellationToken);

            return row.Id;
        }

        /// <summary>
        /// Executes an insert with key-value column data and returns the generated identifier.
        /// </summary>
        /// <typeparam name="T">The identifier type to return.</typeparam>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="data">The column names and values to insert.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The generated identifier.</returns>
        public static T InsertGetId<T>(this Query query, IEnumerable<KeyValuePair<string, object>> data, IDbTransaction transaction = null, int? timeout = null)
        {
            var row = CreateQueryFactory(query).First<InsertGetIdRow<T>>(query.AsInsert(data, true), transaction, timeout);

            return row.Id;
        }

        /// <summary>
        /// Asynchronously executes an insert with key-value column data and returns the generated identifier.
        /// </summary>
        /// <typeparam name="T">The identifier type to return.</typeparam>
        /// <param name="query">The insert query to execute.</param>
        /// <param name="data">The column names and values to insert.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the generated identifier.</returns>
        public static async Task<T> InsertGetIdAsync<T>(this Query query, IEnumerable<KeyValuePair<string, object>> data, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            var row = await CreateQueryFactory(query).FirstAsync<InsertGetIdRow<T>>(query.AsInsert(data, true), transaction, timeout, cancellationToken);

            return row.Id;
        }

        /// <summary>
        /// Executes an update using key-value column data.
        /// </summary>
        /// <param name="query">The update query to execute.</param>
        /// <param name="values">The column names and values to update.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The number of affected rows.</returns>
        public static int Update(this Query query, IEnumerable<KeyValuePair<string, object>> values, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).Execute(query.AsUpdate(values), transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes an update using key-value column data.
        /// </summary>
        /// <param name="query">The update query to execute.</param>
        /// <param name="values">The column names and values to update.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the number of affected rows.</returns>
        public static async Task<int> UpdateAsync(this Query query, IEnumerable<KeyValuePair<string, object>> values, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).ExecuteAsync(query.AsUpdate(values), transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes an update using public properties from an object as column data.
        /// </summary>
        /// <param name="query">The update query to execute.</param>
        /// <param name="data">The object whose public properties supply updated column values.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The number of affected rows.</returns>
        public static int Update(this Query query, object data, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).Execute(query.AsUpdate(data), transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes an update using public properties from an object as column data.
        /// </summary>
        /// <param name="query">The update query to execute.</param>
        /// <param name="data">The object whose public properties supply updated column values.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the number of affected rows.</returns>
        public static async Task<int> UpdateAsync(this Query query, object data, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).ExecuteAsync(query.AsUpdate(data), transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes an increment operation on a numeric column.
        /// </summary>
        /// <param name="query">The update query to execute.</param>
        /// <param name="column">The column to increment.</param>
        /// <param name="value">The amount to add to the column.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The number of affected rows.</returns>
        public static int Increment(this Query query, string column, int value = 1, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).Execute(query.AsIncrement(column, value), transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes an increment operation on a numeric column.
        /// </summary>
        /// <param name="query">The update query to execute.</param>
        /// <param name="column">The column to increment.</param>
        /// <param name="value">The amount to add to the column.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the number of affected rows.</returns>
        public static async Task<int> IncrementAsync(this Query query, string column, int value = 1, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).ExecuteAsync(query.AsIncrement(column, value), transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes a decrement operation on a numeric column.
        /// </summary>
        /// <param name="query">The update query to execute.</param>
        /// <param name="column">The column to decrement.</param>
        /// <param name="value">The amount to subtract from the column.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The number of affected rows.</returns>
        public static int Decrement(this Query query, string column, int value = 1, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).Execute(query.AsDecrement(column, value), transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes a decrement operation on a numeric column.
        /// </summary>
        /// <param name="query">The update query to execute.</param>
        /// <param name="column">The column to decrement.</param>
        /// <param name="value">The amount to subtract from the column.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the number of affected rows.</returns>
        public static async Task<int> DecrementAsync(this Query query, string column, int value = 1, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).ExecuteAsync(query.AsDecrement(column, value), transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes a delete statement for the query.
        /// </summary>
        /// <param name="query">The delete query to execute.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The number of affected rows.</returns>
        public static int Delete(this Query query, IDbTransaction transaction = null, int? timeout = null)
        {
            return CreateQueryFactory(query).Execute(query.AsDelete(), transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes a delete statement for the query.
        /// </summary>
        /// <param name="query">The delete query to execute.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the number of affected rows.</returns>
        public static async Task<int> DeleteAsync(this Query query, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await CreateQueryFactory(query).ExecuteAsync(query.AsDelete(), transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes an aggregate query and returns its scalar value.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The aggregate query to execute.</param>
        /// <param name="aggregateOperation">The aggregate operation name, such as count, avg, sum, min, or max.</param>
        /// <param name="columns">The columns included in the aggregate operation.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The aggregate scalar value.</returns>
        public static T Aggregate<T>(this Query query, string aggregateOperation, string[] columns, IDbTransaction transaction = null, int? timeout = null)
        {
            var db = CreateQueryFactory(query);

            return db.ExecuteScalar<T>(query.AsAggregate(aggregateOperation, columns), transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes an aggregate query and returns its scalar value.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The aggregate query to execute.</param>
        /// <param name="aggregateOperation">The aggregate operation name, such as count, avg, sum, min, or max.</param>
        /// <param name="columns">The columns included in the aggregate operation.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the aggregate scalar value.</returns>
        public static async Task<T> AggregateAsync<T>(this Query query, string aggregateOperation, string[] columns, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            var db = CreateQueryFactory(query);
            return await db.ExecuteScalarAsync<T>(query.AsAggregate(aggregateOperation, columns), transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes a count aggregate for the query.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The query to count.</param>
        /// <param name="columns">The optional columns to count.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The count result.</returns>
        public static T Count<T>(this Query query, string[] columns = null, IDbTransaction transaction = null, int? timeout = null)
        {
            var db = CreateQueryFactory(query);

            return db.ExecuteScalar<T>(query.AsCount(columns), transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes a count aggregate for the query.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The query to count.</param>
        /// <param name="columns">The optional columns to count.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the count value.</returns>
        public static async Task<T> CountAsync<T>(this Query query, string[] columns = null, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            var db = CreateQueryFactory(query);

            return await db.ExecuteScalarAsync<T>(query.AsCount(columns), transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes an average aggregate for the specified column.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The query to aggregate.</param>
        /// <param name="column">The column to average.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The average value.</returns>
        public static T Average<T>(this Query query, string column, IDbTransaction transaction = null, int? timeout = null)
        {
            return query.Aggregate<T>("avg", new[] { column }, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes an average aggregate for the specified column.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The query to aggregate.</param>
        /// <param name="column">The column to average.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the average value.</returns>
        public static async Task<T> AverageAsync<T>(this Query query, string column, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await query.AggregateAsync<T>("avg", new[] { column }, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes a sum aggregate for the specified column.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The query to aggregate.</param>
        /// <param name="column">The column to sum.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The summed value.</returns>
        public static T Sum<T>(this Query query, string column, IDbTransaction transaction = null, int? timeout = null)
        {
            return query.Aggregate<T>("sum", new[] { column }, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes a sum aggregate for the specified column.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The query to aggregate.</param>
        /// <param name="column">The column to sum.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the summed value.</returns>
        public static async Task<T> SumAsync<T>(this Query query, string column, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await query.AggregateAsync<T>("sum", new[] { column }, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes a minimum aggregate for the specified column.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The query to aggregate.</param>
        /// <param name="column">The column to evaluate.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The minimum value.</returns>
        public static T Min<T>(this Query query, string column, IDbTransaction transaction = null, int? timeout = null)
        {
            return query.Aggregate<T>("min", new[] { column }, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes a minimum aggregate for the specified column.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The query to aggregate.</param>
        /// <param name="column">The column to evaluate.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the minimum value.</returns>
        public static async Task<T> MinAsync<T>(this Query query, string column, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await query.AggregateAsync<T>("min", new[] { column }, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Executes a maximum aggregate for the specified column.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The query to aggregate.</param>
        /// <param name="column">The column to evaluate.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <returns>The maximum value.</returns>
        public static T Max<T>(this Query query, string column, IDbTransaction transaction = null, int? timeout = null)
        {
            return query.Aggregate<T>("max", new[] { column }, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes a maximum aggregate for the specified column.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="query">The query to aggregate.</param>
        /// <param name="column">The column to evaluate.</param>
        /// <param name="transaction">The optional database transaction to use.</param>
        /// <param name="timeout">The optional command timeout, in seconds.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task whose result is the maximum value.</returns>
        public static async Task<T> MaxAsync<T>(this Query query, string column, IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await query.AggregateAsync<T>("max", new[] { column }, transaction, timeout, cancellationToken);
        }

        internal static XQuery CastToXQuery(Query query, string method = null)
        {
            var xQuery = query as XQuery;

            if (xQuery is null)
            {
                if (method == null)
                {
                    throw new InvalidOperationException(
                        $"Execution methods can only be used with `{nameof(XQuery)}` instances, " +
                        $"consider using the `{nameof(QueryFactory)}.{nameof(QueryFactory.Query)}()` to create executable queries, " +
                        $"check https://sqlkata.com/docs/execution/setup#xquery-class for more info");
                }

                throw new InvalidOperationException($"The method '{method}()' can only be used with `{nameof(XQuery)}` instances, " +
                    $"consider using the `{nameof(QueryFactory)}.{nameof(QueryFactory.Query)}()` to create executable queries, " +
                    $"check https://sqlkata.com/docs/execution/setup#xquery-class for more info");
            }

            return xQuery;
        }

        internal static QueryFactory CreateQueryFactory(XQuery xQuery)
        {
            QueryFactory factory;

            if (xQuery.QueryFactory != null)
            {
                factory = new QueryFactory(xQuery.Connection, xQuery.Compiler, xQuery.QueryFactory.QueryTimeout);
            }
            else
            {
                factory = new QueryFactory(xQuery.Connection, xQuery.Compiler);
            }

            factory.Logger = xQuery.Logger;

            return factory;
        }

        internal static QueryFactory CreateQueryFactory(Query query)
        {
            return CreateQueryFactory(CastToXQuery(query));
        }
    }
}
