using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface ISqlCreateCommandUtil
    {
        DataSource DataSource { get; }
        string AutoIncrementIdentityCommandGenerator();
        string GetTempTableClause();
    }
}
