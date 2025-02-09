using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableQueryUtils
{
    internal class OracleCreateCommandUtil : ISqlCreateCommandUtil
    {
        public DataSource DataSource { get; } = DataSource.Oracle;

        public string AutoIncrementIdentityCommandGenerator()
        {
            return "GENERATED ALWAYS AS IDENTITY ";
        }
        public string GetTempTableClause()
        {
            return "Global TEMPORARY";
        }
    }
}
