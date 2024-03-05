using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.Abstractions
{
    public interface ICompilerProvider
    {
        Compiler CreateCompiler(DataSource dataSource);
    }
}
