namespace SqlKata
{
    public abstract class AbstractJoin : AbstractClause
    {
    }

    public sealed class BaseJoin : AbstractJoin
    {
        public required Join Join { get; init; }
    }

    public sealed class DeepJoin : AbstractJoin
    {
        public required string Type { get; init; }
        public required string Expression { get; init; }
        public required string SourceKeySuffix { get; init; }
        public required string TargetKey { get; init; }
        public required Func<string, string> SourceKeyGenerator { get; init; }
        public required Func<string, string> TargetKeyGenerator { get; init; }
    }
}
