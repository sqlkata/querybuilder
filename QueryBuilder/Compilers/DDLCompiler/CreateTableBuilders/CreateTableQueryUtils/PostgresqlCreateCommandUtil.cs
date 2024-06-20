using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableQueryUtils
{
    internal class PostgresqlCreateCommandUtil : ISqlCreateCommandUtil
    {
        public DataSource DataSource { get; } = DataSource.Postgresql;

        public string AutoIncrementIdentityCommandGenerator()
        {
            return "";
        }
        public string GetTempTableClause()
        {
            return "TEMPORARY";
        }
    }
}
