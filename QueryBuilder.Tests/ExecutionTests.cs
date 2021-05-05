using System;
using SqlKata.Execution;
using Xunit;

namespace SqlKata.Tests
{
    public class ExecutionTests
    {
        [Fact]
        public void ShouldThrowException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new Query("Books").Get();
            });
        }
    }
}
