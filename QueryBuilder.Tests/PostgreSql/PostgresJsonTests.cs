using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using System;
using Xunit;

namespace SqlKata.Tests.PostgreSql
{
    public class PostgresJsonTests : TestSupport
    {
        private readonly PostgresCompiler regularCompiler = new();
        private readonly JsonAwarePostgresCompiler jsonCompatibleCompiler = new();

        private class JsonAwarePostgresCompiler : PostgresCompiler
        {
            protected override string parameterPlaceholder { get; set; } = "¤";
        }

        [Fact]
        public void LimitWithCustomPlaceHolder()
        {
            var query = new Query("test_table").Limit(10);
            var result = jsonCompatibleCompiler.Compile(query);
            Assert.Equal("""SELECT * FROM "test_table" LIMIT 10""", result.ToString());
        }

        [Fact]
        public void RegularCompilerThrowsExceptionWhereRawJsonContainsQuestionMarkData()
        {
            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                Query query = CreateQuestionMarkJsonQuery(out var rawCondition);
                SqlResult result = regularCompiler.Compile(query);
                Assert.Equal($"""SELECT * FROM "test_table" WHERE {rawCondition}""", result.ToString());
            });
        }

        private Query CreateQuestionMarkJsonQuery(out string rawCondition)
        {
            rawCondition = "my_json_column @> '{\"json_param\" : \"question mark ? \"}'";
            var escapedJsonCondition = rawCondition.Replace("{", "\\{").Replace("}", "\\}");
            return new Query("test_table").WhereRaw(escapedJsonCondition);
        }

        [Fact]
        public void WhereRawJsonWithQuestionMarkData()
        {
            Query query = CreateQuestionMarkJsonQuery(out var rawCondition);
            SqlResult result = jsonCompatibleCompiler.Compile(query);
            Assert.Equal($"""SELECT * FROM "test_table" WHERE {rawCondition}""", result.ToString());
        }

        [Fact]
        public void UsingJsonArray()
        {
            var query = new Query("test_table").WhereRaw("[Json]->'address'->>'country' in (¤)", new[] { 1, 2, 3, 4 });
            SqlResult result = jsonCompatibleCompiler.Compile(query);

            Assert.Equal("""SELECT * FROM "test_table" WHERE "Json"->'address'->>'country' in (1,2,3,4)""", result.ToString());
        }
    }
}
