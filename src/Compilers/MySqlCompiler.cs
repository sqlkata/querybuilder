namespace SqlKata.Compilers
{
    public class MySqlCompiler : Compiler
    {
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
    }
}