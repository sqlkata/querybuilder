using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SqlKata.Tests
{
    public class GenericQueryTest
    {
        private readonly SqlServerCompiler mssqlCompiler;

        public GenericQueryTest()
        {
            mssqlCompiler = new SqlServerCompiler();
        }

        [Fact]
        public void SelectQueryBySelectedColumns()
        {
            var q = new Query<Entity>().Select(x => x.Id, x => x.Name);
            var actual = mssqlCompiler.Compile(q).RawSql;

            string expected = $"SELECT [Id], [Name] FROM [Entity]";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TableNameShouldBeChangable()
        {
            var q = new Query<Entity>().From("Entities");
            var actual = mssqlCompiler.Compile(q).RawSql;

            string exptected = $"SELECT * FROM [Entities]";

            Assert.Equal(exptected, actual);
        }

        [Fact]
        public void OrderBySelectedColumns()
        {
            var q = new Query<Entity>().OrderBy(x => x.Name);
            var actual = mssqlCompiler.Compile(q).RawSql;

            var expected = $"SELECT * FROM [Entity] ORDER BY [Name]";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OrderByDescSelectedColumns()
        {
            var q = new Query<Entity>().OrderByDesc(x => x.Name);
            var actual = mssqlCompiler.Compile(q).RawSql;

            var expected = $"SELECT * FROM [Entity] ORDER BY [Name] DESC";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GroupBySelectedColumns()
        {
            var q = new Query<Entity>().GroupBy(x => x.Name);
            var actual = mssqlCompiler.Compile(q).RawSql;

            var expected = $"SELECT * FROM [Entity] GROUP BY [Name]";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WhereBySelectedColumns()
        {
            var q = new Query<Entity>().Where(x => x.Name == "Test");
            var actual = mssqlCompiler.Compile(q).RawSql;

            var expected = $"SELECT * FROM [Entity] WHERE [Name] = 'Test')";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WhereNotBySelectedColumns()
        {
            var q = new Query<Entity>().WhereNot(x => x.Name == "Test");
            var actual = mssqlCompiler.Compile(q).RawSql;

            var expected = $"SELECT * FROM [Entity] WHERE NOT([Name] = 'Test')";

            Assert.Equal(expected, actual);
        }

    }

    public class Entity
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
