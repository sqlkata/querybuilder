using System.Collections;
using System.Collections.Generic;

namespace SqlKata.Execution
{
    /// <summary>
    /// Enumerates paginated results, beginning with a supplied first page and
    /// loading each subsequent page as enumeration advances.
    /// </summary>
    /// <typeparam name="T">The type of each item in the paginated results.</typeparam>
    public class PaginationIterator<T> : IEnumerable<PaginationResult<T>>
    {
        /// <summary>
        /// Gets or sets the page from which enumeration starts.
        /// </summary>
        public PaginationResult<T> FirstPage { get; set; }

        /// <summary>
        /// Gets or sets the page most recently returned by the iterator.
        /// </summary>
        public PaginationResult<T> CurrentPage { get; set; }

        /// <summary>
        /// Returns an enumerator that yields the first page and loads later pages on demand.
        /// </summary>
        /// <returns>An enumerator over the available pages.</returns>
        public IEnumerator<Execution.PaginationResult<T>> GetEnumerator()
        {
            CurrentPage = FirstPage;

            yield return CurrentPage;

            while (CurrentPage.HasNext)
            {
                CurrentPage = CurrentPage.Next();
                yield return CurrentPage;
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
