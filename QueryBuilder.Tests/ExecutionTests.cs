using SqlKata.Execution;

namespace SqlKata.Tests
{
    public class ExecutionTests
    {
        [Fact]
        public void Get_WhenWasNotCalledOnXQuery_ShouldThrowException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new Query("Books").Get();
            });
        }

        [Fact]
        public void TimeoutShouldBeCarriedToNewCreatedFactory()
        {
            var db = new QueryFactory();
            db.QueryTimeout = 4000;
            var newFactory = QueryExtensions.CreateQueryFactory(db.Query());
            Assert.Equal(db.QueryTimeout, newFactory.QueryTimeout);
        }

        [Fact]
        public void TimeoutShouldBeCarriedToNewCreatedFactoryAfterClone()
        {
            var db = new QueryFactory();
            db.QueryTimeout = 4000;
            var newFactory = QueryExtensions.CreateQueryFactory(db.Query().Clone());
            Assert.Equal(db.QueryTimeout, newFactory.QueryTimeout);
        }
    }
}
