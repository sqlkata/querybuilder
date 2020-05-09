namespace SqlKata
{
    public enum ClauseComponent : byte
    {
        Select = 0,
        Insert,
        Update,
        Where,
        From,
        Having,
        Cte,
        Combine,
        Aggregate,
        Limit,
        Order,
        Group,
        Join,
        Offset
    }
}