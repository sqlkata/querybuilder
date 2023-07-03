using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableQueryUtils
{
    internal class MySqlCreateCommandUtil : ISqlCreateCommandUtil
    {
        public string AutoIncrementIdentityCommandGenerator()
        {
            return "AUTO_INCREMENT ";
        }

        public string CreateTableCommandGenerator(TableType tableType, string tableName)
        {
            if (tableType == TableType.Temporary)
                return $"CREATE TEMPORARY TABLE {tableName}  ";
            return $"CREATE TABLE {tableName}  ";
        }
    }
}
