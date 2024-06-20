using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface ISqlCreateCommandProvider
    {
        ISqlCreateCommandUtil GetSqlCreateCommandUtil(DataSource dataSource);
    }
}
