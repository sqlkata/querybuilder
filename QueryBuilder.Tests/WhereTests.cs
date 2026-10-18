using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public class WhereTests : TestSupport
    {
        [Fact]
        public void GroupedWhereFilters()
        {
            var q = new Query("Table1")
                .Where(q => q.Or().Where("Column1", 10).Or().Where("Column2", 20))
                .Where("Column3", 30);

            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM ""Table1"" WHERE (""Column1"" = 10 OR ""Column2"" = 20) AND ""Column3"" = 30", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void GroupedHavingFilters()
        {
            var q = new Query("Table1")
                .Having(q => q.Or().HavingRaw("SUM([Column1]) = ?", 10).Or().HavingRaw("SUM([Column2]) = ?", 20))
                .HavingRaw("SUM([Column3]) = ?", 30);

            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM ""Table1"" HAVING (SUM(""Column1"") = 10 OR SUM(""Column2"") = 20) AND SUM(""Column3"") = 30", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void GroupedHavingWithConditionGroup()
        {
            var group = new ConditionGroup();
            group.Where("Total", ">", 100).OrWhere("Count", "<", 5);

            var q = new Query("Orders")
                .Having(group)
                .HavingBetween("Average", 10, 50)
                .HavingIn("Category", new[] { 1, 2, 3 });

            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM [Orders] HAVING ([Total] > 100 OR [Count] < 5) AND [Average] BETWEEN 10 AND 50 AND [Category] IN (1, 2, 3)", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void ConditionGroupStandalone()
        {
            var group = new ConditionGroup();
            group.Where("A", 1).OrWhere("B", 2);

            Assert.Equal(2, group.Clauses.Count);
            Assert.False(((AbstractCondition)group.Clauses[0]).IsOr);
            Assert.True(((AbstractCondition)group.Clauses[1]).IsOr);
        }

        [Fact]
        public void ConditionGroup_HasConditions_DeepCheck()
        {
            var query = new Query("Users")
                .Where(q => q.For(EngineCodes.Oracle, q2 => q2.Where("Age", ">", 18)));

            Assert.True(query.Wheres.HasConditions(EngineCodes.Oracle));
            Assert.False(query.Wheres.HasConditions(EngineCodes.SqlServer));

            var group = new ConditionGroup();
            var inner = new ConditionGroup { EngineScope = EngineCodes.Oracle };
            inner.Where("Age", ">", 18);
            group.Clauses.Add(new NestedCondition { Conditions = inner });

            Assert.True(group.HasConditions(EngineCodes.Oracle));
            Assert.False(group.HasConditions(EngineCodes.SqlServer));
        }

        [Fact]
        public void Or_FollowedByWhere_DoesNotAffectSubsequentHaving()
        {
            var q = new Query("Table1")
                .Where("A", 10)
                .Or()
                .Where("B", 20)
                .HavingRaw("SUM([C]) = ?", 1)
                .HavingRaw("SUM([D]) = ?", 2);

            var c = Compile(q);

            Assert.True(((AbstractCondition)q.Wheres.Clauses[1]).IsOr);
            Assert.False(((AbstractCondition)q.Havings.Clauses[1]).IsOr);
            Assert.Equal(
                @"SELECT * FROM [Table1] WHERE [A] = 10 OR [B] = 20 HAVING SUM([C]) = 1 AND SUM([D]) = 2",
                c[EngineCodes.SqlServer]
            );
        }

        [Fact]
        public void Or_FollowedByHaving_DoesNotAffectSubsequentWhere()
        {
            var q = new Query("Table1")
                .HavingRaw("SUM([C]) = ?", 1)
                .Or()
                .HavingRaw("SUM([D]) = ?", 2)
                .Where("A", 10)
                .Where("B", 20);

            var c = Compile(q);

            Assert.True(((AbstractCondition)q.Havings.Clauses[1]).IsOr);
            Assert.False(((AbstractCondition)q.Wheres.Clauses[1]).IsOr);
            Assert.Equal(
                @"SELECT * FROM [Table1] WHERE [A] = 10 AND [B] = 20 HAVING SUM([C]) = 1 OR SUM([D]) = 2",
                c[EngineCodes.SqlServer]
            );
        }

        [Fact]
        public void Or_Interleaved_WhereThenHavingThenWhere()
        {
            var q = new Query("Table1")
                .Where("A", 1)
                .Or()
                .HavingRaw("SUM([C]) = ?", 2)
                .Where("B", 3);

            var c = Compile(q);

            // The Or() was consumed by Having, so B remains AND
            Assert.False(((AbstractCondition)q.Wheres.Clauses[1]).IsOr);
            Assert.Equal(
                @"SELECT * FROM [Table1] WHERE [A] = 1 AND [B] = 3 HAVING SUM([C]) = 2",
                c[EngineCodes.SqlServer]
            );
        }

        [Fact]
        public void Or_Interleaved_HavingThenWhereThenHaving()
        {
            var q = new Query("Table1")
                .HavingRaw("SUM([C]) = ?", 1)
                .Or()
                .Where("A", 2)
                .HavingRaw("SUM([D]) = ?", 3);

            var c = Compile(q);

            // The Or() was consumed by Where, so SUM(D) remains AND
            Assert.False(((AbstractCondition)q.Havings.Clauses[1]).IsOr);
            Assert.Equal(
                @"SELECT * FROM [Table1] WHERE [A] = 2 HAVING SUM([C]) = 1 AND SUM([D]) = 3",
                c[EngineCodes.SqlServer]
            );
        }

        [Fact]
        public void CallbackSubqueries_ResolveOuterVariables_WhereIn()
        {
            var q = new Query("Orders")
                .Define("tenantId", 42)
                .WhereIn("UserId", sub => sub.From("Users").Where("TenantId", Expressions.Variable("tenantId")));

            var c = Compilers.Compile(q);

            Assert.Equal("SELECT * FROM [Orders] WHERE [UserId] IN (SELECT * FROM [Users] WHERE [TenantId] = ?)", c[EngineCodes.SqlServer].RawSql);
            Assert.Contains(42, c[EngineCodes.SqlServer].Bindings);
        }

        [Fact]
        public void CallbackSubqueries_ResolveOuterVariables_Where()
        {
            var q = new Query("Orders")
                .Define("tenantId", 42)
                .Where("Total", ">", sub => sub.From("Stats").Select("AvgTotal").Where("TenantId", Expressions.Variable("tenantId")));

            var c = Compilers.Compile(q);

            Assert.Contains(42, c[EngineCodes.SqlServer].Bindings);
        }

        [Fact]
        public void CallbackSubqueries_ResolveOuterVariables_WhereExists()
        {
            var q = new Query("Orders")
                .Define("tenantId", 42)
                .WhereExists(sub => sub.From("Users").Where("TenantId", Expressions.Variable("tenantId")));

            var c = Compilers.Compile(q);

            Assert.Contains(42, c[EngineCodes.SqlServer].Bindings);
        }

        [Fact]
        public void CallbackSubqueries_ResolveOuterVariables_Join()
        {
            var q = new Query("Orders")
                .Define("tenantId", 42)
                .Join("Users", j => j.On("Orders.UserId", "Users.Id")
                    .WhereIn("Users.TenantId", sub => sub.From("Tenants").Select("Id").Where("Id", Expressions.Variable("tenantId"))));

            var c = Compilers.Compile(q);

            Assert.Contains(42, c[EngineCodes.SqlServer].Bindings);
        }

        [Fact]
        public void ExternalSubquery_ResolvesOuterVariables_WithoutParentConnection()
        {
            // sub is completely isolated, no Parent or callbacks used
            var sub = new Query("Users").Where("TenantId", Expressions.Variable("tenantId"));

            var q = new Query("Orders")
                .Define("tenantId", 42)
                .WhereIn("UserId", sub);

            var c = Compilers.Compile(q);

            Assert.Equal("SELECT * FROM [Orders] WHERE [UserId] IN (SELECT * FROM [Users] WHERE [TenantId] = ?)", c[EngineCodes.SqlServer].RawSql);
            Assert.Contains(42, c[EngineCodes.SqlServer].Bindings);
        }

        [Fact]
        public void CallbackSubqueries_ResolveOuterVariables_From()
        {
            var q = new Query()
                .Define("tenantId", 42)
                .From(sub => sub.From("Users").Where("TenantId", Expressions.Variable("tenantId")), "u");

            var c = Compilers.Compile(q);

            Assert.Equal("SELECT * FROM (SELECT * FROM [Users] WHERE [TenantId] = ?) AS [u]", c[EngineCodes.SqlServer].RawSql);
            Assert.Contains(42, c[EngineCodes.SqlServer].Bindings);
        }

        [Fact]
        public void ExternalSubquery_ResolveOuterVariables_From()
        {
            var sub = new Query("Users").Where("TenantId", Expressions.Variable("tenantId"));

            var q = new Query()
                .Define("tenantId", 42)
                .From(sub, "u");

            var c = Compilers.Compile(q);

            Assert.Equal("SELECT * FROM (SELECT * FROM [Users] WHERE [TenantId] = ?) AS [u]", c[EngineCodes.SqlServer].RawSql);
            Assert.Contains(42, c[EngineCodes.SqlServer].Bindings);
        }

        [Fact]
        public void SqlServer_LegacyPagination_InSubquery_ResolvesOuterVariables()
        {
            var compiler = new SqlServerCompiler { UseLegacyPagination = true };

            var q = new Query("Orders")
                .Define("tenantId", 42)
                .WhereIn("UserId", sub => sub.From("Users")
                    .Where("TenantId", Expressions.Variable("tenantId"))
                    .OrderBy("Id")
                    .Offset(10)
                    .Limit(5));

            var result = compiler.Compile(q);

            Assert.Contains(42, result.Bindings);
        }

        [Fact]
        public void NestedCondition_ScopedToAnotherEngine_ShouldNotProduceBareWhere()
        {
            var query = new Query("Users")
                .Where(q => q.For(EngineCodes.Oracle, q2 => q2.Where("Age", ">", 18)));

            var compiler = new SqlServerCompiler();
            var result = compiler.Compile(query);

            Assert.Equal("SELECT * FROM [Users]", result.RawSql);
        }

        [Fact]
        public void NestedCondition_ScopedToAnotherEngine_ShouldNotProduceBareHaving()
        {
            var query = new Query("Users")
                .Having(q => q.For(EngineCodes.Oracle, q2 => q2.Having("Age", ">", 18)));

            var compiler = new SqlServerCompiler();
            var result = compiler.Compile(query);

            Assert.Equal("SELECT * FROM [Users]", result.RawSql);
        }

        [Fact]
        public void WhereIn_EmptyCollection_ShouldEmitConstantPredicateWithoutColumn()
        {
            var query = new Query("Users").WhereIn("id", System.Array.Empty<int>());

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Users] WHERE 1 = 0 /* IN [empty list] */", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM \"Users\" WHERE 1 = 0 /* IN [empty list] */", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void WhereNotIn_EmptyCollection_ShouldEmitConstantPredicateWithoutColumn()
        {
            var query = new Query("Users").WhereNotIn("id", System.Array.Empty<int>());

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Users] WHERE 1 = 1 /* NOT IN [empty list] */", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM \"Users\" WHERE 1 = 1 /* NOT IN [empty list] */", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void WhereIn_EmptyCollection_CombinedWithOtherConditions()
        {
            var query = new Query("Users")
                .Where("Status", "Active")
                .WhereIn("id", System.Array.Empty<int>());

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Users] WHERE [Status] = 'Active' AND 1 = 0 /* IN [empty list] */", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Subquery_AggregateFilter_ResolvesOuterVariables()
        {
            var q = new Query("Orders")
                .Define("tenantId", 42)
                .Where("Total", ">", sub => sub.From("Stats")
                    .SelectSum("Amount", f => f.Where("TenantId", Expressions.Variable("tenantId"))));

            var c = Compilers.Compile(q);

            Assert.Contains(42, c[EngineCodes.SqlServer].Bindings);
            Assert.Equal("SELECT * FROM [Orders] WHERE [Total] > (SELECT SUM(CASE WHEN [TenantId] = ? THEN [Amount] END) FROM [Stats])", c[EngineCodes.SqlServer].RawSql);
            Assert.Contains(42, c[EngineCodes.PostgreSql].Bindings);
            Assert.Equal("SELECT * FROM \"Orders\" WHERE \"Total\" > (SELECT SUM(\"Amount\") FILTER (WHERE \"TenantId\" = ?) FROM \"Stats\")", c[EngineCodes.PostgreSql].RawSql);
        }
    }
}
