namespace SqlKata
{
    public interface IRaw
    {
        string Expression { get; }
        object[] Bindings { get; }
    }
}