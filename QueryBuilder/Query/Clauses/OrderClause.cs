using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractOrderBy : AbstractClause
    {
    }

    public sealed class OrderBy : AbstractOrderBy
    {
        public required string Column { get; init; }
        public required bool Ascending { get; init; }
    }

    public sealed class RawOrderBy : AbstractOrderBy
    {
        public required string Expression { get; init; }
        public required ImmutableArray<object> Bindings { get; init; }
    }

    public sealed class OrderByRandom : AbstractOrderBy
    {
    }
}
