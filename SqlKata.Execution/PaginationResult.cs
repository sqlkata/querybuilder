using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlKata.Execution
{
    public class PaginationResult<T>
    {
        /// <summary>
        ///     Returns a page <see cref="PaginationIterator{T}" />
        /// </summary>
        public PaginationIterator<T> Each => new PaginationIterator<T>
        {
            FirstPage = this
        };

        /// <summary>
        ///     Queries the database for the next page
        /// </summary>
        /// <returns></returns>
        public Query NextQuery()
        {
            return Query.ForPage(Page + 1, PerPage);
        }

        /// <summary>
        ///     Move to the next page
        /// </summary>
        /// <returns></returns>
        public PaginationResult<T> Next()
        {
            return Query.Paginate<T>(Page + 1, PerPage);
        }

        /// <summary>
        ///     Move the the next page async
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationResult<T>> NextAsync()
        {
            return await Query.PaginateAsync<T>(Page + 1, PerPage);
        }

        /// <summary>
        ///     Queries the database for the previous page
        /// </summary>
        /// <returns></returns>
        public Query PreviousQuery()
        {
            return Query.ForPage(Page - 1, PerPage);
        }

        /// <summary>
        ///     Move the the previous page
        /// </summary>
        /// <returns></returns>
        public PaginationResult<T> Previous()
        {
            return Query.Paginate<T>(Page - 1, PerPage);
        }

        /// <summary>
        ///     Move to the previous page async
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationResult<T>> PreviousAsync()
        {
            return await Query.PaginateAsync<T>(Page - 1, PerPage);
        }

        #region Properties
        /// <summary>
        ///     The <see cref="Query" />
        /// </summary>
        public Query Query { get; internal set; }

        /// <summary>
        ///     Returns the total number of pages
        /// </summary>
        public long Count { get; internal set; }

        /// <summary>
        ///     Returns an <see cref="IEnumerable{T}" /> list
        /// </summary>
        public IEnumerable<T> List { get; internal set; }

        /// <summary>
        ///     Returns the current page
        /// </summary>
        public int Page { get; internal set; }

        /// <summary>
        ///     Returns the amount of records on each page
        /// </summary>
        public int PerPage { get; internal set; }

        /// <summary>
        ///     Returns the total amount of pages
        /// </summary>
        public int TotalPages
        {
            get
            {
                if (PerPage < 1)
                {
                    return 0;
                }

                var div = (float) Count / PerPage;

                return (int) Math.Ceiling(div);
            }
        }

        /// <summary>
        ///     Returns <c>true</c> when this is the first page
        /// </summary>
        public bool IsFirst => Page == 1;

        /// <summary>
        ///     Returns <c>true</c> when this is the last page
        /// </summary>
        public bool IsLast => Page == TotalPages;

        /// <summary>
        ///     Returns <c>true</c> when there is a next page
        /// </summary>
        public bool HasNext => Page < TotalPages;

        /// <summary>
        ///     Returns <c>true</c> when there is a previous page
        /// </summary>
        public bool HasPrevious => Page > 1;
        #endregion
    }
}