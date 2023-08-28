using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public sealed class IntermediateStageSelectTests : TestSupport
    {
        [Fact]
        public void BasicSelect()
        {
            CompareWithCompiler(new Query().From("users").Select("id", "name"));
        }
    }
}
