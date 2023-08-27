using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractInsertClause : AbstractClause
    {
    }

    public class InsertClause : AbstractInsertClause
    {
        public required ImmutableArray<string> Columns { get; set; }
        public required ImmutableArray<object?> Values { get; set; }
        public required bool ReturnId { get; set; }
    }

    public class InsertQueryClause : AbstractInsertClause
    {
        public required ImmutableArray<string> Columns { get; set; }
        public required Query Query { get; set; }
    }
}
