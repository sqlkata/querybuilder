using SqlKata.Compilers;

namespace SqlKata.Tests.Infrastructure
{
    /// <summary>
    /// A test class to expose private methods
    /// </summary>
    class TestCompiler : Compiler
    {
        public override string EngineCode { get; } = "test";
    }

    class TestSqlServerCompiler : SqlServerCompiler
    {
    }

    class TestMySqlCompiler : MySqlCompiler
    {
    }

    class TestPostgresCompiler : PostgresCompiler
    {
    }

    class TestFirebirdCompiler : FirebirdCompiler
    {
    }

    class TestEmptyIdentifiersCompiler : TestCompiler
    {
        protected override string OpeningIdentifier { get; set; } = "";
        protected override string ClosingIdentifier { get; set; } = "";
    }
}
