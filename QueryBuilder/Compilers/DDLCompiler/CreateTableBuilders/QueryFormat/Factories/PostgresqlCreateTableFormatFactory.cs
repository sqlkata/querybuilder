using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Factories
{
    internal class PostgresqlCreateTableFormatFactory : ICreateQueryFormatFactory
    {
        public DataSource DataSource { get; } = DataSource.Postgresql;
        public string CreateTableFormat()
        {
            return @"CREATE {0} TABLE {1} (
                {2},
                {3}
                {4}
            )";
        }
    }
}
