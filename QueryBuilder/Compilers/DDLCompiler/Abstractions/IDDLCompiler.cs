using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    public interface IDDLCompiler
    {
        SqlResult CompileCreateTable(Query query,DataSource dataSource);
        SqlResult CompileCreateTableAs(Query query, DataSource dataSource,string compiledSelectQuery);
    }
}
