using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableQueryUtils
{
    internal class MySqlCreateCommandUtil : ISqlCreateCommandUtil
    {
        public DataSource DataSource { get; } = DataSource.MySql;

        public string AutoIncrementIdentityCommandGenerator()
        {
            return "AUTO_INCREMENT ";
        }

        public string GetTempTableClause()
        {
            return "TEMPORARY";
        }
    }
}
