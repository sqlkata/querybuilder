namespace SqlKata
{
    public sealed class IncrementClause : AbstractInsertClause
    {
        public required string Column { get; init; }
        public required int Value { get; init; } 
    }
}
