namespace SqlKata.QueryBuilder
{
    public interface IRaw
    {
        string Expression { get; set; }
        object[] Bindings { set; }
    }
}