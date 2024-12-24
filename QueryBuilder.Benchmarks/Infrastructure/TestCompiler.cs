using SqlKata.Compilers;

namespace QueryBuilder.Benchmarks.Infrastructure;

public class TestCompiler
    : Compiler
{
    public override string EngineCode { get; } = "generic";
}
