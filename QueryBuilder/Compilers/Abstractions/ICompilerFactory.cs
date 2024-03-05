using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.Abstractions
{
    internal interface ICompilerFactory
    {
        DataSource DataSource { get; }
        Compiler CreateCompiler();
    }
}
