using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface ICreateTableAsCompiler
    {
        string CompileCreateAsQuery(Query query, DataSource dataSource,string compiledSelectQuery);
    }
}
