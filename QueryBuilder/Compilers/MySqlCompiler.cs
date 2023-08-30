namespace SqlKata.Compilers
{
    public class MySqlCompiler : Compiler
    {
        public MySqlCompiler()
        {
            XService = new X("`", "`", "AS ");
            LastId = "SELECT last_insert_id() as Id";
            EngineCode = EngineCodes.MySql;
        }

        protected override string? CompileLimit(SqlResult ctx, Writer writer)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (offset == 0 && limit == 0) return null;

            if (offset == 0)
            {
                ctx.BindingsAdd(limit);
                writer.S.Append("LIMIT ?");
                return writer;
            }

            if (limit == 0)
            {
                // MySql will not accept offset without limit, so we will put a large number
                // to avoid this error.

                ctx.BindingsAdd(offset);
                writer.S.Append("LIMIT 18446744073709551615 OFFSET ?");
                return writer;
            }

            // We have both values

            ctx.BindingsAdd(limit);
            ctx.BindingsAdd(offset);

            writer.S.Append("LIMIT ? OFFSET ?");
            return writer;
        }
    }
}
