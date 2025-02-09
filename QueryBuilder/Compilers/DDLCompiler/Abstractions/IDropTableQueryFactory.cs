namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    public interface IDropTableQueryFactory
    {
        string CompileQuery(Query query);
    }
}
