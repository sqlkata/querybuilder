using SqlKata.Compilers;

namespace SqlKata.Tests.Infrastructure;

/// <summary>
///     A test class to expose private methods
/// </summary>
internal class TestCompiler : Compiler
{
    protected TestCompiler()
    {
        EngineCode = "test";
    }

}

internal class TestEmptyIdentifiersCompiler : TestCompiler
{
    public TestEmptyIdentifiersCompiler()
    {
        XService = new X("", "", "AS ");
    }
}
