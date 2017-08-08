namespace SqlKata.Compilers
{
    public class MySqlCompiler : Compiler
    {
        public MySqlCompiler() : base()
        {
            EngineCode = "mysql";
        }

        /// <summary>
        /// Wrap a single string in keyword identifiers.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override string WrapValue(string value)
        {
            if (value == "*") return value;

            return '`' + value.Replace("`", "``") + '`';
        }

        public override string CompileOffset(Query query)
        {
            var limitOffset = query.GetOne("limit", EngineCode) as LimitOffset;

            if (limitOffset == null || !limitOffset.HasOffset())
            {
                return "";
            }

            // MySql will not accept offset without limit
            // So we will put a large number to avoid this error
            if (!limitOffset.HasLimit())
            {
                return "LIMIT 18446744073709551615 OFFSET ?";
            }

            return "OFFSET ?";
        }
    }
}