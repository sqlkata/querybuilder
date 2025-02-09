using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface ICreateTableAsFormatFiller
    {
        string FillCreateTableAsQuery(string queryFormat,string compiledSelectQuery,Query query,DataSource dataSource);
    }
}
