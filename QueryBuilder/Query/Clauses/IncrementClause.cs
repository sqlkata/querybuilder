namespace SqlKata
{
    public sealed class IncrementClause : InsertClause
    {
        public required string Column { get; init; }
        public required int Value { get; init; } 
    }
}
