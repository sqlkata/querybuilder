using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface ISqlCreateCommandUtil
    {
        string CreateTableCommandGenerator(TableType tableType,string tableName);
        string AutoIncrementIdentityCommandGenerator();
    }
}
