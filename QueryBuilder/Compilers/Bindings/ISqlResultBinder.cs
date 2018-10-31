namespace SqlKata.Compilers.Bindings
{
    public interface ISqlResultBinder
    {
        void BindNamedParameters(SqlResult sqlResult);
    }
}