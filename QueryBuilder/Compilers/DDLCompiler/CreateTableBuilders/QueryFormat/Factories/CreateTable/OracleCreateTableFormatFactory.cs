using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Factories.CreateTable
{
    internal class OracleCreateTableFormatFactory : ICreateQueryFormatFactory
    {
        public DataSource DataSource { get; } = DataSource.Oracle;
        public string CreateTableFormat()
        {
            return @"CREATE {0} {1} TABLE {2} (
{3}{4}{5})
{6}";
        }
    }
}
