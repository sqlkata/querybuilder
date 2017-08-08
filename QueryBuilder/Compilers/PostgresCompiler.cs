namespace SqlKata.Compilers
{
    public class PostgresCompiler : Compiler
    {
        public PostgresCompiler() : base()
        {
            EngineCode = "postgres";
        }
    }
}