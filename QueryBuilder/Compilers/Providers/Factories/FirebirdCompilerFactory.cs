using SqlKata.Compilers.Abstractions;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.Providers.Factories
{
    public class FirebirdCompilerFactory: ICompilerFactory
    {
        private readonly IDDLCompiler _ddlCompiler;

        public FirebirdCompilerFactory(IDDLCompiler ddlCompiler)
        {
            _ddlCompiler = ddlCompiler;
        }

        public DataSource DataSource { get; } = DataSource.Firebird;
        public Compiler CreateCompiler()
        {
            return new FirebirdCompiler(_ddlCompiler);
        }
    }
}
