namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    public interface ITruncateTableQueryFactory
    {
        string CompileQuery(Query query);
    }
}
