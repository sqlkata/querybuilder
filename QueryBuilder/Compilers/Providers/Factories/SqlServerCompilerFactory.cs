using SqlKata.Compilers.Abstractions;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.Providers.Factories
{
    public class SqlServerCompilerFactory : ICompilerFactory
    {
        private readonly IDDLCompiler _ddlCompiler;

        public SqlServerCompilerFactory(IDDLCompiler ddlCompiler)
        {
            _ddlCompiler = ddlCompiler;
        }

        public DataSource DataSource { get; } = DataSource.SqlServer;
        public Compiler CreateCompiler()
        {
            return new SqlServerCompiler(_ddlCompiler);
        }
    }
}
