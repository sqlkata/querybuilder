namespace SqlKata.QueryBuilder.Clauses
{
    public interface RawInterface
    {
        string Expression { get; set; }
        object[] Bindings { set; }
    }
}
