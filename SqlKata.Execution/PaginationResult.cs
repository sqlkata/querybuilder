using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SqlKata;

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

        public bool IsFirst => Page == 1;

        public bool IsLast => Page == TotalPages;

        public bool HasNext => Page < TotalPages;

        public bool HasPrevious => Page > 1;

        public Query NextQuery()
        {
            return Query.ForPage(Page + 1, PerPage);
        }

        public PaginationResult<T> Next()
        {
            return Query.Paginate<T>(Page + 1, PerPage);
        }

        public async Task<PaginationResult<T>> NextAsync()
        {
            return await Query.PaginateAsync<T>(Page + 1, PerPage);
        }

        public Query PreviousQuery()
        {
            return Query.ForPage(Page - 1, PerPage);
        }

        public PaginationResult<T> Previous()
        {
            return Query.Paginate<T>(Page - 1, PerPage);
        }

        public async Task<PaginationResult<T>> PreviousAsync()
        {
            return await Query.PaginateAsync<T>(Page - 1, PerPage);
        }

        public PaginationIterator<T> Each => new PaginationIterator<T>
        {
            FirstPage = this
        };
    }
}