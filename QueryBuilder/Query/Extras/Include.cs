namespace SqlKata
{
    public sealed class Include
    {
        public required string Name { get; init; }
        public required Query Query { get; init; }
        public required string LocalKey { get; init; }
        public required bool IsMany { get; init; }

        public required string? ForeignKey { get; set; }
    }
}
