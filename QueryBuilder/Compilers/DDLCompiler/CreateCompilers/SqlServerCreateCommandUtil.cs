using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateCompilers
{
    internal class SqlServerCreateCommandUtil : ISqlCreateCommandUtil
    {
        public string AutoIncrementIdentityCommandGenerator()
        {
            return "IDENTITY(1,1) ";
        }

        public string CreateTableCommandGenerator(TableType tableType, string tableName)
        {
            return $"CREATE TABLE {tableName}  ";
        }
    }
}
