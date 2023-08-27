namespace SqlKata.Compilers
{
    public static class CompilationExtensions
    {
        public static SqlResult Compile(this Compiler compiler, Query query)
        {
            var ctx = compiler.CompileRaw(query);
            ctx = compiler.PrepareResult(ctx);
            return ctx;
        }
    }
}
