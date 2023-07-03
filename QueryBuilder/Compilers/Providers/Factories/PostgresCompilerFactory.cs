using SqlKata.Compilers.Abstractions;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.Providers.Factories
{
    public class PostgresCompilerFactory : ICompilerFactory
    {
        private readonly IDDLCompiler _ddlCompiler;

        public PostgresCompilerFactory(IDDLCompiler ddlCompiler)
        {
            _ddlCompiler = ddlCompiler;
        }

        public DataSource DataSource { get; } = DataSource.Postgresql;
        public Compiler CreateCompiler()
        {
            return new PostgresCompiler(_ddlCompiler);
        }
    }
}
