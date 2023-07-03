using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableQueryUtils
{
    internal class OracleCreateCommandUtil : ISqlCreateCommandUtil
    {
        public string AutoIncrementIdentityCommandGenerator()
        {
            return "GENERATED ALWAYS AS IDENTITY ";
        }
    }
}
