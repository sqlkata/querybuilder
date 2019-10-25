namespace SqlKata
{
    public enum QueryMethod : byte
    {
        Select = 0,
        Insert,
        Update,
        Delete,
        Aggregate
    }
}