using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface ICreateQueryFormatFiller
    {
        DataSource DataSource { get; }
        string FillQueryFormat(string queryFormat,Query query);
    }
}
