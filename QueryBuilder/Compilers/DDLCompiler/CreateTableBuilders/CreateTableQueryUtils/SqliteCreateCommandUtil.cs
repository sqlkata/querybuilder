using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableQueryUtils
{
    internal class SqliteCreateCommandUtil : ISqlCreateCommandUtil
    {
        public DataSource DataSource { get; } = DataSource.Sqlite;

        public string AutoIncrementIdentityCommandGenerator()
        {
            return "AUTOINCREMENT ";
        }

        public string GetTempTableClause()
        {
            return "TEMPORARY";
        }
    }
}
