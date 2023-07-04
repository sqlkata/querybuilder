using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Factories.CreateTable
{
    internal class MySqlCreateTableFormatFactory : ICreateQueryFormatFactory
    {
        public DataSource DataSource { get; } = DataSource.MySql;
        public string CreateTableFormat()
        {
            return @"CREATE {0} TABLE {1} (
                {2}
                {3}
                {4}
            )";
        }
    }
}
