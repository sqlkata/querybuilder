using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface IDDLCompiler
    {
        SqlResult CompileCreateTable(Query query,DataSource dataSource);
    }
}
