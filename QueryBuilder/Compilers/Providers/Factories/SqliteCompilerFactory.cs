using SqlKata.Compilers.Abstractions;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.Providers.Factories
{
    public class SqliteCompilerFactory : ICompilerFactory
    {
        private readonly IDDLCompiler _ddlCompiler;

        public SqliteCompilerFactory(IDDLCompiler ddlCompiler)
        {
            _ddlCompiler = ddlCompiler;
        }

        public DataSource DataSource { get; } = DataSource.Sqlite;
        public Compiler CreateCompiler()
        {
            return new SqliteCompiler(_ddlCompiler);
        }
    }
}
