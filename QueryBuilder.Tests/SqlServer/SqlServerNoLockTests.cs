using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using System;
using Xunit;

namespace SqlKata.Tests.SqlServer
{
    public class SqlServerNoLockTrueTests : SqlServerNoLockTests
    {
        public SqlServerNoLockTrueTests() : base(true)
        {
        }
    }

    public class SqlServerNoLockFalseTests : SqlServerNoLockTests
    {
        public SqlServerNoLockFalseTests() : base(false)
        {
        }
    }

    public abstract class SqlServerNoLockTests : TestSupport
    {
        protected string _suffix;
        protected readonly SqlServerCompiler compiler;

        public SqlServerNoLockTests(bool useNoLock)
        {
            compiler = Compilers.Get<SqlServerCompiler>(EngineCodes.SqlServer);
            compiler.UseNoLock = useNoLock;
            _suffix = useNoLock ? " WITH (NOLOCK)" : String.Empty;
        }

        
        [Fact]
        public void Select()
        {
            var q = new Query("table");
            var c = compiler.Compile(q);

            Assert.Equal($"SELECT * FROM [table]{_suffix}", c.ToString());
        }

        [Fact]
        public void SelectAs()
        {
            var q = new Query("table as t");
            var c = compiler.Compile(q);

            Assert.Equal($"SELECT * FROM [table] AS [t]{_suffix}", c.ToString());
        }

        [Fact]
        public void Join()
        {
            var q = new Query("table").Join("other", "a", "b");
            var c = compiler.Compile(q);

            Assert.Equal($"SELECT * FROM [table]{_suffix} \nINNER JOIN [other]{_suffix} ON [a] = [b]", c.ToString());
        }

        [Fact]
        public void JoinAs()
        {
            var q = new Query("table").Join("other as o", "a", "b");
            var c = compiler.Compile(q);

            Assert.Equal($"SELECT * FROM [table]{_suffix} \nINNER JOIN [other] AS [o]{_suffix} ON [a] = [b]", c.ToString());
        }

        [Fact]
        public void Cte()
        {
            var cte = new Query("table");
            var q = new Query("other as o")
                .With("cte", cte)
                .Join("cte", "o.id", "cte.id");
            var c = compiler.Compile(q);

            Assert.Equal($"WITH [cte] AS (SELECT * FROM [table]{_suffix})\nSELECT * FROM [other] AS [o]{_suffix} \nINNER JOIN [cte]{_suffix} ON [o].[id] = [cte].[id]",
                c.ToString());
        }


        // To show there's no change to Insert and Delete
        [Fact]
        public void Delete()
        {
            var q = new Query("table").AsDelete();
            var c = compiler.Compile(q);

            Assert.Equal("DELETE FROM [table]", c.ToString());
        }

        [Fact]
        public void Insert()
        {
            var q = new Query("table").AsInsert(new { id = 5 });
            var c = compiler.Compile(q);

            Assert.Equal("INSERT INTO [table] ([id]) VALUES (5)", c.ToString());
        }
    }
}
