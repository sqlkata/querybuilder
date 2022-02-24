using System;
using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests
{

    public class SqlServerDeleteTest
    {
        private SqlServerCompiler compiler = new SqlServerCompiler();

        [Fact]
        public void PlainDeleteNoRegression()
        {
            var query = new Query("Table").AsDelete();
            var r = compiler.Compile(query);

            Assert.Equal("DELETE FROM [Table]", r.Sql);
        }

        [Fact]
        public void PlainDeleteWhereNoRegression()
        {
            var query = new Query("Table")
                .Where("IsDeleted",1)
                .WhereIn("TableId",new[]{1,2,3})
                .AsDelete();
            var r = compiler.Compile(query);

            Assert.Equal("DELETE FROM [Table] WHERE [IsDeleted] = 1 AND [TableId] IN (1, 2, 3)", r.ToString());
        }

        [Fact]
        public void DeleteFromRawIsUnsupported()
        {
            var query = new Query()
                .FromRaw("[Complex_Table.Name]")
                .AsDelete();
            Assert.Throws<InvalidOperationException>(() => { compiler.Compile(query); });
        }

        [Fact]
        public void DeleteJoinSimple()
        {
            var query = new Query("Table")
                .Join("Section", "Table.SectionId", "Section.SectionId")
                .AsDelete();

            var r = compiler.Compile(query);

            Assert.Equal(
                "DELETE [Table] FROM [Table] \nINNER JOIN [Section] ON [Table].[SectionId] = [Section].[SectionId]",
                r.ToString());
        }

        [Fact]
        public void DeleteJoinWithSchema()
        {
            var query = new Query("Audit.Table")
                .Join("Audit.Section", "Table.SectionId", "Section.SectionId")
                .AsDelete();

            var r = compiler.Compile(query);

            Assert.Equal(
                "DELETE [Table] FROM [Audit].[Table] \nINNER JOIN [Audit].[Section] ON [Table].[SectionId] = [Section].[SectionId]",
                r.ToString());
        }

        [Fact]
        public void DeleteJoinWithSchemaAndAlias()
        {
            var query = new Query("Audit.Table AS auditTable")
                .Join("Audit.Section as auditSection", "auditTable.SectionId", "auditSection.SectionId")
                .AsDelete();

            var r = compiler.Compile(query);

            Assert.Equal(
                "DELETE [auditTable] FROM [Audit].[Table] AS [auditTable] \nINNER JOIN [Audit].[Section] AS [auditSection] ON [auditTable].[SectionId] = [auditSection].[SectionId]",
                r.ToString());
        }

    }
}
