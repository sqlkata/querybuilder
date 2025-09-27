using SqlKata.Compilers;
using SqlKata.Extensions;
using SqlKata.Tests.Infrastructure;
using System;
using System.Linq;
using Xunit;

namespace SqlKata.Tests
{
	public class CloneTests : TestSupport
	{
		[Fact]
		public void Clone_creates_deep_copy_of_query_components()
		{
			var postsQuery = new Query("Posts").Where("Published", true);
			var query = new Query("Users")
				.Select("Id")
				.Where("Active", true)
				.As("u")
				.Distinct()
				.Define("limit", 10)
				.IncludeMany("posts", postsQuery, foreignKey: "UserId", localKey: "Id");

			var originalColumnClause = query.Clauses.OfType<Column>().First();

			var clone = query.Clone();

			Assert.NotSame(query, clone);
			Assert.Equal("u", clone.QueryAlias);
			Assert.True(clone.IsDistinct);
			Assert.Equal(query.Method, clone.Method);

			Assert.NotSame(query.Clauses, clone.Clauses);
			Assert.Equal(query.Clauses.Count, clone.Clauses.Count);
			for (var index = 0; index < query.Clauses.Count; index++)
			{
				Assert.NotSame(query.Clauses[index], clone.Clauses[index]);
				Assert.Equal(query.Clauses[index].GetType(), clone.Clauses[index].GetType());
			}

			originalColumnClause.Name = "Users.Id as Identifier";
			var clonedColumnName = clone.Clauses.OfType<Column>().First().Name;
			Assert.Equal("Id", clonedColumnName);

			Assert.NotSame(query.Variables, clone.Variables);
			Assert.Equal(10, clone.Variables["limit"]);
			query.Variables["limit"] = 100;
			Assert.Equal(10, clone.Variables["limit"]);

			Assert.Single(clone.Includes);
			var originalInclude = query.Includes.Single();
			var cloneInclude = clone.Includes.Single();
			Assert.NotSame(originalInclude, cloneInclude);
			Assert.NotSame(originalInclude.Query, cloneInclude.Query);

			var cloneIncludeClauseCount = cloneInclude.Query.Clauses.Count;
			originalInclude.Query.Where("Pinned", true);
			Assert.Equal(cloneIncludeClauseCount, cloneInclude.Query.Clauses.Count);
		}

		[Fact]
		public void Clone_clones_parent_chain_independently()
		{
			var root = new Query("Users")
				.Where("Active", true)
				.As("users");

			var child = root.NewChild()
				.Select("Id")
				.Where("Age", ">", 30);

			var clone = child.Clone();

			var originalParent = Assert.IsType<Query>(child.Parent);
			var clonedParent = Assert.IsType<Query>(clone.Parent);

			Assert.NotSame(originalParent, clonedParent);
			Assert.Equal(originalParent.QueryAlias, clonedParent.QueryAlias);
			Assert.Equal(originalParent.Clauses.Count, clonedParent.Clauses.Count);

			var compiler = new SqlServerCompiler();
			var originalParentSql = compiler.Compile(originalParent).Sql;
			var clonedParentSql = compiler.Compile(clonedParent).Sql;
			Assert.Equal(originalParentSql, clonedParentSql);

			originalParent.Where("Archived", false);
			var mutatedParentSql = compiler.Compile(originalParent).Sql;
			var clonedParentSqlAfterMutation = compiler.Compile(clonedParent).Sql;
			Assert.NotEqual(mutatedParentSql, clonedParentSqlAfterMutation);

			Assert.Null(clonedParent.Parent);
			Assert.NotSame(child.Clauses, clone.Clauses);
			Assert.Equal(child.Clauses.Count, clone.Clauses.Count);
		}
	}
}
