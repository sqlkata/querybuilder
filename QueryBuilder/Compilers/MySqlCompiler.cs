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

        protected override string? CompileLimit(SqlResult ctx, Query query, Writer writer)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (offset == 0 && limit == 0) return null;

            if (offset == 0)
            {
                ctx.BindingsAdd(limit);
                writer.Append("LIMIT ?");
                return writer;
            }

            if (limit == 0)
            {
                // MySql will not accept offset without limit, so we will put a large number
                // to avoid this error.

                ctx.BindingsAdd(offset);
                writer.Append("LIMIT 18446744073709551615 OFFSET ?");
                return writer;
            }

            // We have both values

            ctx.BindingsAdd(limit);
            ctx.BindingsAdd(offset);

            writer.Append("LIMIT ? OFFSET ?");
            return writer;
        }
    }
}
