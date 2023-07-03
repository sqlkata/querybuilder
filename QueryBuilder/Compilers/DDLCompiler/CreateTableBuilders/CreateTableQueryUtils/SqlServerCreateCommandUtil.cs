using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableQueryUtils
{
    internal class SqlServerCreateCommandUtil : ISqlCreateCommandUtil
    {
        public string AutoIncrementIdentityCommandGenerator()
        {
            return "IDENTITY(1,1) ";
        }
    }
}
