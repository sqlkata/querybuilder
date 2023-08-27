using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractInsertClause : AbstractClause
    {
    }

    public class InsertClause : AbstractInsertClause
    {
        public required ImmutableArray<string> Columns { get; init; }
        public required ImmutableArray<object?> Values { get; init; }
        public required bool ReturnId { get; init; }
    }

    public sealed class InsertQueryClause : AbstractInsertClause
    {
        public required ImmutableArray<string> Columns { get; init; }
        public required Query Query { get; init; }
    }
}
