using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface ICreateTableQueryFillerProvider
    {
        ICreateQueryFormatFiller GetCreateQueryFormatFiller(DataSource dataSource);
    }
}
