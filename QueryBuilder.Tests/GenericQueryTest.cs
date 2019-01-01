using SqlKata.Compilers;
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
        public void SelectQueryBySelectedColumnsTest()
        {
            var q = new Query<Entity>().Select(x => new { x.Id, x.Name, x.Country });
            var actual = mssqlCompiler.Compile(q).RawSql;

            string expected = $"SELECT [Id], [Name], [Country] FROM [Entity]";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TableNameShouldBeChangeableTest()
        {
            var q = new Query<Entity>().From("Entities");
            var actual = mssqlCompiler.Compile(q).RawSql;

            string exptected = $"SELECT * FROM [Entities]";

            Assert.Equal(exptected, actual);
        }

        [Fact]
        public void OrderBySelectedColumnsTest()
        {
            var q = new Query<Entity>().OrderBy(x => new { x.Name });
            var qMultiple = new Query<Entity>().OrderBy(x => new { x.Name, x.Country });

            var actual = mssqlCompiler.Compile(q).RawSql;
            var actualMultiple = mssqlCompiler.Compile(qMultiple).RawSql;

            var expected = $"SELECT * FROM [Entity] ORDER BY [Name]";
            var expectedMultiple = $"SELECT * FROM [Entity] ORDER BY [Name], [Country]";

            Assert.Equal(expected, actual);

            Assert.Equal(expectedMultiple, actualMultiple);
        }

        [Fact]
        public void OrderByDescSelectedColumnsTest()
        {
            var q = new Query<Entity>().OrderByDesc(x => new { x.Name });
            var qMultiple = new Query<Entity>().OrderByDesc(x => new { x.Name, x.Country });

            var actual = mssqlCompiler.Compile(q).RawSql;
            var actualMultiple = mssqlCompiler.Compile(qMultiple).RawSql;

            var expected = $"SELECT * FROM [Entity] ORDER BY [Name] DESC";
            var expectedMultiple = $"SELECT * FROM [Entity] ORDER BY [Name] DESC, [Country] DESC";

            Assert.Equal(expected, actual);

            Assert.Equal(expectedMultiple, actualMultiple);
        }

        [Fact]
        public void GroupBySelectedColumnsTest()
        {
            var q = new Query<Entity>().GroupBy(x => x.Name);
            var actual = mssqlCompiler.Compile(q).RawSql;

            var expected = $"SELECT * FROM [Entity] GROUP BY [Name]";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InnerJoinTest()
        {
            var q = new Query<Entity>().Join<JoinEntity>((x, y) => x.Id == y.EntityId);
            var q2 = new Query<Entity>().Join<JoinEntity>((x, y) => new { x.Id, y.EntityId });

            var actual = mssqlCompiler.Compile(q).RawSql;
            var actual2 = mssqlCompiler.Compile(q2).RawSql;

            var expected = $"SELECT * FROM [Entity] \nINNER JOIN [JoinEntity] ON [Id] = [EntityId]";

            Assert.Equal(expected, actual);

            Assert.Equal(expected, actual2);
        }

        [Fact]
        public void InnerJoin_TableNamesShouldBeChangeableTest()
        {
            var q = new Query<Entity>().From("Entities").Join<JoinEntity>("JoinEntities", (x, y) => x.Id == y.EntityId);

            var actual = mssqlCompiler.Compile(q).RawSql;

            var expected = $"SELECT * FROM [Entities] \nINNER JOIN [JoinEntities] ON [Id] = [EntityId]";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LeftJoinTest()
        {
            var q = new Query<Entity>().LeftJoin<JoinEntity>((x, y) => x.Id == y.EntityId);
            var q2 = new Query<Entity>().LeftJoin<JoinEntity>((x, y) => new { x.Id, y.EntityId });

            var actual = mssqlCompiler.Compile(q).RawSql;
            var actual2 = mssqlCompiler.Compile(q2).RawSql;

            var expected = $"SELECT * FROM [Entity] \nLEFT JOIN [JoinEntity] ON [Id] = [EntityId]";

            Assert.Equal(expected, actual);

            Assert.Equal(expected, actual2);

        }

        [Fact]
        public void LeftJoin_TableNamesShouldBeChangeableTest()
        {
            var q = new Query<Entity>().From("Entities").LeftJoin<JoinEntity>("JoinEntities", (x, y) => x.Id == y.EntityId);

            var actual = mssqlCompiler.Compile(q).RawSql;

            var expected = $"SELECT * FROM [Entities] \nLEFT JOIN [JoinEntities] ON [Id] = [EntityId]";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RightJoinTest()
        {
            var q = new Query<Entity>().RightJoin<JoinEntity>((x, y) => x.Id == y.EntityId);
            var q2 = new Query<Entity>().RightJoin<JoinEntity>((x, y) => new { x.Id, y.EntityId });

            var actual = mssqlCompiler.Compile(q).RawSql;
            var actual2 = mssqlCompiler.Compile(q2).RawSql;

            var expected = $"SELECT * FROM [Entity] \nRIGHT JOIN [JoinEntity] ON [Id] = [EntityId]";

            Assert.Equal(expected, actual);

            Assert.Equal(expected, actual2);
        }

        [Fact]
        public void RightJoin_TableNamesShouldBeChangeableTest()
        {
            var q = new Query<Entity>().From("Entities").RightJoin<JoinEntity>("JoinEntities", (x, y) => x.Id == y.EntityId);

            var actual = mssqlCompiler.Compile(q).RawSql;

            var expected = $"SELECT * FROM [Entities] \nRIGHT JOIN [JoinEntities] ON [Id] = [EntityId]";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CrossJoinTest()
        {
            var q = new Query<Entity>().CrossJoin<JoinEntity>();

            var actual = mssqlCompiler.Compile(q).RawSql;

            var expected = $"SELECT * FROM [Entity] \nCROSS JOIN [JoinEntity]";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CrossJoin_TableNamesShouldBeChangeableTest()
        {
            var q = new Query<Entity>().From("Entities").CrossJoin<JoinEntity>("JoinEntities");

            var actual = mssqlCompiler.Compile(q).RawSql;

            var expected = $"SELECT * FROM [Entities] \nCROSS JOIN [JoinEntities]";

            Assert.Equal(expected, actual);
        }
    }

    public class Entity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string Country { get; set; }
    }

    public class JoinEntity
    {
        public int Id { get; set; }

        public int EntityId { get; set; }

        public string Name { get; set; }
    }
}
