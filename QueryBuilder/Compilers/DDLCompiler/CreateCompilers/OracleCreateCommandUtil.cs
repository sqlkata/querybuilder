using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateCompilers
{
    internal class OracleCreateCommandUtil : ISqlCreateCommandUtil
    {
        public string AutoIncrementIdentityCommandGenerator()
        {
            return "GENERATED ALWAYS AS IDENTITY ";
        }

        public string CreateTableCommandGenerator(TableType tableType, string tableName)
        {
            if (tableType == TableType.Temporary)
                return $"CREATE GLOBAL TEMPORARY TABLE {tableName} ";
            else
                return $"CREATE TABLE {tableName}   ";
        }
    }
}
