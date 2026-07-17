using System.Linq;
using SqlKata.Execution;
using Xunit;

namespace SqlKata.Tests
{
    public class PaginationResultTests
    {
        [Fact]
        public void ReportsPageStateFromCountAndPageSize()
        {
            var result = new PaginationResult<int>
            {
                Count = 25,
                Page = 2,
                PerPage = 10,
            };

            Assert.Equal(3, result.TotalPages);
            Assert.False(result.IsFirst);
            Assert.False(result.IsLast);
            Assert.True(result.HasNext);
            Assert.True(result.HasPrevious);
        }

        [Fact]
        public void BuildsQueriesForAdjacentPages()
        {
            var nextResult = new PaginationResult<int>
            {
                Query = new Query("Posts"),
                Page = 2,
                PerPage = 10,
            };

            var nextQuery = nextResult.NextQuery();

            Assert.Same(nextResult.Query, nextQuery);
            Assert.Equal(20, nextQuery.GetOffset());
            Assert.Equal(10, nextQuery.GetLimit());

            var previousResult = new PaginationResult<int>
            {
                Query = new Query("Posts"),
                Page = 2,
                PerPage = 10,
            };

            var previousQuery = previousResult.PreviousQuery();

            Assert.Same(previousResult.Query, previousQuery);
            Assert.Equal(0, previousQuery.GetOffset());
            Assert.Equal(10, previousQuery.GetLimit());
        }

        [Fact]
        public void EachYieldsTheCurrentPageWhenThereIsNoNextPage()
        {
            var result = new PaginationResult<int>
            {
                Count = 2,
                List = new[] { 1, 2 },
                Page = 1,
                PerPage = 10,
            };

            var iterator = result.Each;
            var pages = iterator.ToList();

            Assert.Single(pages);
            Assert.Same(result, pages[0]);
            Assert.Same(result, iterator.CurrentPage);
        }
    }
}
