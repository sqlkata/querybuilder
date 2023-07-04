using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Factories.CreateTable
{
    internal class SqlServerCreateTableFormatFactory : ICreateQueryFormatFactory
    {
        public DataSource DataSource { get; } = DataSource.SqlServer;
        public string CreateTableFormat()
        {
            return  @"CREATE TABLE {0}(
                        {1}
                        {2}
                        {3}
                    )
                    ";
        }
    }
}
