using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface ISqlCreateCommandUtil
    {
        string AutoIncrementIdentityCommandGenerator();
    }
}
