using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using System;
using Xunit;

namespace SqlKata.Tests.PostgreSql
{
    public class PostgresJsonTests : TestSupport
    {
        public class JsonAwarePostgresCompiler : PostgresCompiler
        {
            public override string ParameterPlaceholder { get; protected set; } = "$$";
        }

        private readonly JsonAwarePostgresCompiler compiler = new();
        private PostgresCompiler regularCompiler;

        public PostgresJsonTests()
        {
            regularCompiler = Compilers.Get<PostgresCompiler>(EngineCodes.PostgreSql);
        }

        [Fact]
        public void LimitWithCustomPlaceHolder()
        {
            var query = new Query("Table").Limit(10);
            var ctx = new SqlResult(compiler) { Query = query };

            Assert.Equal($"LIMIT $$", compiler.CompileLimit(ctx));
            Assert.Equal(10, ctx.Bindings[0]);
        }

        [Fact]
        public void RegularCompilerThrowsExceptionWhereRawJsonContainsQuestionMarkData()
        {
            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                Query query = CreateQuestionMarkJsonQuery(out var rawCondition);

                SqlResult result = regularCompiler.Compile(query);
                Assert.Equal($"SELECT * FROM \"Table\" WHERE {rawCondition}", result.ToString());
            });
        }

        private Query CreateQuestionMarkJsonQuery(out string rawCondition)
        {
            rawCondition = "my_json_column @> '{\"json_param\" : \"question mark ? \"}'";
            var escapedJsonCondition = rawCondition.Replace("{", "\\{").Replace("}", "\\}");
            return new Query("Table").WhereRaw(escapedJsonCondition);
        }

        [Fact]
        public void WhereRawJsonWithQuestionMarkData()
        {
            Query query = CreateQuestionMarkJsonQuery(out var rawCondition);
            SqlResult result = compiler.Compile(query);
            Assert.Equal($"SELECT * FROM \"Table\" WHERE {rawCondition}", result.ToString());
        }

        [Fact]
        public void UsingJsonArray()
        {
            var query = new Query("Table").WhereRaw("[Json]->'address'->>'country' in ($$)", new[] { 1, 2, 3, 4 });

            SqlResult result = compiler.Compile(query);

            Assert.Equal("SELECT * FROM \"Table\" WHERE \"Json\"->'address'->>'country' in (1,2,3,4)", result.ToString());
        }
    }
}
