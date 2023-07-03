using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableQueryUtils
{
    internal class SqlServerCreateCommandUtil : ISqlCreateCommandUtil
    {
        public DataSource DataSource { get; } = DataSource.SqlServer;

        public string AutoIncrementIdentityCommandGenerator()
        {
            return "IDENTITY(1,1) ";
        }
    }
}
