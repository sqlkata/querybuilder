using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SqlKata.Execution
{
    public class PaginationResult<T>
    {
        public Query Query { get; set; }
        public long Count { get; set; }
        public IEnumerable<T> List { get; set; }
        public int Page { get; set; }
        public int PerPage { get; set; }
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

        public bool IsFirst
        {
            get
            {
                return Page == 1;
            }
        }

        public bool IsLast
        {
            get
            {
                return Page == TotalPages;
            }
        }

        public bool HasNext
        {
            get
            {
                return Page < TotalPages;
            }
        }

        public bool HasPrevious
        {
            get
            {
                return Page > 1;
            }
        }

        public Query NextQuery()
        {
            return this.Query.ForPage(Page + 1, PerPage);
        }

        public PaginationResult<T> Next(IDbTransaction transaction = null, int? timeout = null)
        {
            return this.Query.Paginate<T>(Page + 1, PerPage, transaction, timeout);
        }

        public async Task<PaginationResult<T>> NextAsync(IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await this.Query.PaginateAsync<T>(Page + 1, PerPage, transaction, timeout, cancellationToken);
        }

        public Query PreviousQuery()
        {
            return this.Query.ForPage(Page - 1, PerPage);
        }

        public PaginationResult<T> Previous(IDbTransaction transaction = null, int? timeout = null)
        {
            return this.Query.Paginate<T>(Page - 1, PerPage, transaction, timeout);
        }

        public async Task<PaginationResult<T>> PreviousAsync(IDbTransaction transaction = null, int? timeout = null, CancellationToken cancellationToken = default)
        {
            return await this.Query.PaginateAsync<T>(Page - 1, PerPage, transaction, timeout, cancellationToken);
        }

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
