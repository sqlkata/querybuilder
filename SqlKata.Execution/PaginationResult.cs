using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SqlKata.Execution
{
    /// <summary>
    /// Represents one page of query results together with pagination metadata
    /// and helpers for loading adjacent pages.
    /// </summary>
    /// <typeparam name="T">The type of each item in the result set.</typeparam>
    public class PaginationResult<T>
    {
        /// <summary>
        /// Gets or sets the query used to produce the paginated results.
        /// </summary>
        public Query Query { get; set; }

        /// <summary>
        /// Gets or sets the total number of matching records across all pages.
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// Gets or sets the records in the current page.
        /// </summary>
        public IEnumerable<T> List { get; set; }

        /// <summary>
        /// Gets or sets the one-based current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of records per page.
        /// </summary>
        public int PerPage { get; set; }

        /// <summary>
        /// Gets the total number of pages, rounded up from <see cref="Count"/>.
        /// </summary>
        public int TotalPages
        {
            get
            {

                if (PerPage < 1)
                {
                    return 0;
                }

                var div = (float)Count / PerPage;

                return (int)Math.Ceiling(div);

            }
        }

        /// <summary>
        /// Gets a value indicating whether this is the first page.
        /// </summary>
        public bool IsFirst
        {
            get
            {
                return Page == 1;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this is the last page.
        /// </summary>
        public bool IsLast
        {
            get
            {
                return Page == TotalPages;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a page follows the current page.
        /// </summary>
        public bool HasNext
        {
            get
            {
                return Page < TotalPages;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a page precedes the current page.
        /// </summary>
        public bool HasPrevious
        {
            get
            {
                return Page > 1;
            }
        }

        /// <summary>
        /// Applies the next page's limit and offset to <see cref="Query"/>.
        /// </summary>
        /// <returns>The query configured for the next page.</returns>
        public Query NextQuery()
        {
            return this.Query.ForPage(Page + 1, PerPage);
        }

        /// <summary>
        /// Executes the query for the next page.
        /// </summary>
        /// <param name="transaction">The transaction to use, or <see langword="null"/> for none.</param>
        /// <param name="timeout">The command timeout in seconds, or <see langword="null"/> to use the configured default.</param>
        /// <returns>The next page of results.</returns>
        public PaginationResult<T> Next(IDbTransaction transaction = null, int? timeout = null)
        {
            return this.Query.Paginate<T>(Page + 1, PerPage, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query for the next page.
        /// </summary>
        /// <param name="transaction">The transaction to use, or <see langword="null"/> for none.</param>
        /// <param name="timeout">The command timeout in seconds, or <see langword="null"/> to use the configured default.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>A task containing the next page of results.</returns>
        public async Task<PaginationResult<T>> NextAsync(IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await this.Query.PaginateAsync<T>(Page + 1, PerPage, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Applies the previous page's limit and offset to <see cref="Query"/>.
        /// </summary>
        /// <returns>The query configured for the previous page.</returns>
        public Query PreviousQuery()
        {
            return this.Query.ForPage(Page - 1, PerPage);
        }

        /// <summary>
        /// Executes the query for the previous page.
        /// </summary>
        /// <param name="transaction">The transaction to use, or <see langword="null"/> for none.</param>
        /// <param name="timeout">The command timeout in seconds, or <see langword="null"/> to use the configured default.</param>
        /// <returns>The previous page of results.</returns>
        public PaginationResult<T> Previous(IDbTransaction transaction = null, int? timeout = null)
        {
            return this.Query.Paginate<T>(Page - 1, PerPage, transaction, timeout);
        }

        /// <summary>
        /// Asynchronously executes the query for the previous page.
        /// </summary>
        /// <param name="transaction">The transaction to use, or <see langword="null"/> for none.</param>
        /// <param name="timeout">The command timeout in seconds, or <see langword="null"/> to use the configured default.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>A task containing the previous page of results.</returns>
        public async Task<PaginationResult<T>> PreviousAsync(IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await this.Query.PaginateAsync<T>(Page - 1, PerPage, transaction, timeout, cancellationToken);
        }

        /// <summary>
        /// Gets an iterator that starts with this page and loads each remaining page.
        /// </summary>
        public PaginationIterator<T> Each
        {
            get
            {
                return new PaginationIterator<T>
                {
                    FirstPage = this
                };
            }
        }

    }
}
