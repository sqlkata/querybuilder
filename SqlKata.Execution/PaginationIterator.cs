using System.Collections;
using System.Collections.Generic;

namespace SqlKata.Execution
{
    public class PaginationIterator<T> : IEnumerable<PaginationResult<T>>
    {
        #region Properties
        public PaginationResult<T> FirstPage { get; internal set; }
        public PaginationResult<T> CurrentPage { get; internal set; }
        #endregion

        public IEnumerator<PaginationResult<T>> GetEnumerator()
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