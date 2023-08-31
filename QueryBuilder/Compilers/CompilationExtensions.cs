namespace SqlKata.Compilers
{
    public static class CompilationExtensions
    {
        public static SqlResult Compile(this Compiler compiler, Query query)
        {
            var writer = new Writer(compiler.XService);
            compiler.CompileRaw(query, writer);
            var ctx = new SqlResult();
            ctx.ReplaceRaw(writer);
            ctx.ReplaceBindings(writer.Bindings);
            ctx = compiler.PrepareResult(ctx, writer);
            return ctx;
        }

        public static SqlResult Compile(this Compiler compiler, IEnumerable<Query> queries)
        {
            var writer = new Writer(compiler.XService);
            var ctx = Accumulate();

            ctx.ReplaceRaw(writer);
            ctx = compiler.PrepareResult(ctx, writer);
            return ctx;

            SqlResult Accumulate()
            {
                var sqlResult = new SqlResult();
                writer.List(";\n", queries, query =>
                {
                    compiler.CompileRaw(query, writer);
                });
                return sqlResult;
            }
        }

        private static SqlResult PrepareResult(this Compiler compiler, SqlResult ctx, Writer writer)
        {
            ctx.NamedBindings = ctx.Bindings.GenerateNamedBindings(compiler.ParameterPrefix);
            ctx.Sql = BindingExtensions.ReplaceAll(writer,
                "?", i => compiler.ParameterPrefix + i);
            writer.EnsureBindingMatch();
            return ctx;
        }
    }
}
