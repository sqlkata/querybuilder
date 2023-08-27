using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractFrom : AbstractClause
    {
        protected string? AliasField;

        /// <summary>
        ///     Try to extract the Alias for the current clause.
        /// </summary>
        /// <returns></returns>
        public virtual string? Alias
        {
            get => AliasField;
            set => AliasField = value;
        }
    }

    /// <summary>
    ///     Represents a "from" clause.
    /// </summary>
    public sealed class FromClause : AbstractFrom
    {
        public required string Table { get; init; }

        public override string Alias
        {
            get
            {
                if (Table.IndexOf(" as ", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var segments = Table.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    return segments[2];
                }

                return Table;
            }
        }
    }

    /// <summary>
    ///     Represents a "from subQuery" clause.
    /// </summary>
    public sealed class QueryFromClause : AbstractFrom
    {
        public required Query Query { get; init; }

        public override string? Alias => string.IsNullOrEmpty(AliasField) ? Query.QueryAlias : AliasField;
    }

    public sealed class RawFromClause : AbstractFrom
    {
        public required string Expression { get; init; }
        public required ImmutableArray<object> Bindings { get; init; }
    }

    /// <summary>
    ///     Represents a FROM clause that is an ad-hoc table built with predefined values.
    /// </summary>
    public sealed class AdHocTableFromClause : AbstractFrom
    {
        public ImmutableArray<string> Columns { get; init; }
        public ImmutableArray<object> Values { get; init; }
    }
}
