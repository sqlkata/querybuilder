using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface ICreateQueryFormatFactory
    {
        DataSource DataSource { get; }
        string CreateTableFormat();
    }
}
