using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public abstract class AbstractFrom : AbstractClause
    {
    }

    /// <summary>
    ///     Represents a "from" clause.
    /// </summary>
    public sealed class FromClause : AbstractFrom
    {
        public required string Table { get; init; }
        public required string Alias { get; init; }

        public override void Render(StringBuilder sb, Renderer r)
        {
            r.X.Wrap(sb, Table);
        }
    }

    /// <summary>
    ///     Represents a "from subQuery" clause.
    /// </summary>
    public sealed class QueryFromClause : AbstractFrom
    {
        public required string? Alias { get; init; }
        public required Query Query { get; init; }
    }

    public sealed class RawFromClause : AbstractFrom
    {
        public required string? Alias { get; init; }
        public required string Expression { get; init; }
        public required ImmutableArray<object> Bindings { get; init; }
        
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(r.X.WrapIdentifiers(Expression));
        }
    }

    /// <summary>
    ///     Represents a FROM clause that is an ad-hoc table built with predefined values.
    /// </summary>
    public sealed class AdHocTableFromClause : AbstractFrom
    {
        public required string Alias { get; init; }
        public required ImmutableArray<string> Columns { get; init; }
        public required ImmutableArray<object?> Values { get; init; }
    }
}
