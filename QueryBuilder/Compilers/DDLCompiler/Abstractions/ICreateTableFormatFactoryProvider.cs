using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface ICreateTableFormatFactoryProvider
    {
        ICreateQueryFormatFactory GetCreateQueryFormatFactory(DataSource dataSource);
    }
}
