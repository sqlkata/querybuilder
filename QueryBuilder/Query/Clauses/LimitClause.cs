namespace SqlKata
{
    public sealed class LimitClause : AbstractClause
    {
        public required int Limit { get; init; }
    }
}
