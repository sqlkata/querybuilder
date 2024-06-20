using SqlKata.Compilers.Abstractions;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.Providers.Factories
{
    public class OracleCompilerFactory: ICompilerFactory
    {
        private readonly IDDLCompiler _ddlCompiler;

        public OracleCompilerFactory(IDDLCompiler ddlCompiler)
        {
            _ddlCompiler = ddlCompiler;
        }

        public DataSource DataSource { get; } = DataSource.Oracle;
        public Compiler CreateCompiler()
        {
            return new OracleCompiler(_ddlCompiler);
        }
    }
}
