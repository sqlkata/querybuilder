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

        public override string CompileLimit(SqlResult context)
        {
            int limit = context.Query.GetLimit(EngineCode);
            int offset = context.Query.GetOffset(EngineCode);


            if (offset == 0 && limit == 0)
            {
                return null;
            }

            if (offset == 0)
            {
                context.Bindings.Add(limit);
                return "LIMIT ?";
            }

            if (limit == 0)
            {

                // MySql will not accept offset without limit, so we will put a large number
                // to avoid this error.

                context.Bindings.Add(offset);
                return "LIMIT 18446744073709551615 OFFSET ?";
            }

            // We have both values

            context.Bindings.Add(limit);
            context.Bindings.Add(offset);

            return "LIMIT ? OFFSET ?";
        }
    }
}