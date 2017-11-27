using System;
using System.Collections.Generic;

namespace SqlKata
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

                var div = (double)(Count / PerPage);

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

        public PaginationResult<T> Next()
        {
            return this.Query.Paginate<T>(Page + 1, PerPage);
        }

        public PaginationResult<T> Previous()
        {
            return this.Query.Paginate<T>(Page - 1, PerPage);
        }

        public Query NextQuery()
        {
            return this.Query.ForPage(Page + 1, PerPage);
        }

        public Query PreviousQuery()
        {
            return this.Query.ForPage(Page - 1, PerPage);
        }

    }
}