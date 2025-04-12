using System.Collections.Generic;

namespace SqlKata.Execution
{
    public class ScrollToResult<T>
    {
        public Query Query { get; set; }
        public long Count { get; set; }
        public IEnumerable<T> List { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
