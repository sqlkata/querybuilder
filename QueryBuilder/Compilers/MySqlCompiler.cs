namespace SqlKata.Compilers
{
    public class MySqlCompiler : Compiler
    {
        public MySqlCompiler()
        {
            OpeningIdentifier = ClosingIdentifier = "`";
            LastId = "SELECT last_insert_id() as Id";
        }

        public override string EngineCode { get; } = EngineCodes.MySql;

        public override void VisitLimit(CompileContext ctx, Query query)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (offset == 0 && limit == 0)
            {
                return;
            }

            ctx.EnsureSpace();

            if (offset == 0)
            {
                ctx.Bindings.Add(limit);
                ctx.Write($"LIMIT {parameterPlaceholder}");
                return;
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                ctx.Write($"LIMIT 18446744073709551615 OFFSET {parameterPlaceholder}");
                return;
            }

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);
            ctx.Write($"LIMIT {parameterPlaceholder} OFFSET {parameterPlaceholder}");
        }
    }
}
