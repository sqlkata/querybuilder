namespace SqlKata.Tests.Infrastructure.TestCompilers;

class TestEmptyIdentifiersCompiler : TestCompiler
{
    protected override string OpeningIdentifier { get; set; } = "";
    protected override string ClosingIdentifier { get; set; } = "";
}
