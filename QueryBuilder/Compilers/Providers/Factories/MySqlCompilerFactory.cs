using SqlKata.Compilers.Abstractions;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.Providers.Factories
{
    public class MySqlCompilerFactory : ICompilerFactory
    {
        private readonly IDDLCompiler _ddlCompiler;

        public MySqlCompilerFactory(IDDLCompiler ddlCompiler)
        {
            _ddlCompiler = ddlCompiler;
        }

        public DataSource DataSource { get; } = DataSource.MySql;
        public Compiler CreateCompiler()
        {
            return new MySqlCompiler(_ddlCompiler);
        }
    }
}
