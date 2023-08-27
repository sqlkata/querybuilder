namespace SqlKata
{
    public sealed class OffsetClause : AbstractClause
    {
        public required long Offset { get; init; }
    }
}
