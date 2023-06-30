using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateCompilers
{
    internal class SqliteCreateColmmandUtil : ISqlCreateCommandUtil
    {
        public string AutoIncrementIdentityCommandGenerator()
        {
            return "AUTOINCREMENT ";
        }

        public string CreateTableCommandGenerator(TableType tableType, string tableName)
        {
            if (tableType == TableType.Temporary)
                return $"CREATE TEMPORARY TABLE {tableName}";
            return $"CREATE TABLE {tableName}";
        }
    }
}
