namespace SqlKata
{
    public abstract class AbstractClause
    {
        public required string? Engine { get; init; }
        public required string Component { get; init; }
    }
}
