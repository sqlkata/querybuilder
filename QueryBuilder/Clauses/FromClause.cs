using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractFrom : AbstractClause
    {
        protected string AliasField;

        /// <summary>
        ///     Try to extract the Alias for the current clause.
        /// </summary>
        /// <returns></returns>
        public virtual string Alias
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
        public string Table { get; set; }

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
    public class QueryFromClause : AbstractFrom
    {
        public Query Query { get; set; }

        public override string Alias => string.IsNullOrEmpty(AliasField) ? Query.QueryAlias : AliasField;
    }

    public class RawFromClause : AbstractFrom
    {
        public string Expression { get; set; }
        public object[] Bindings { set; get; }
    }

    /// <summary>
    ///     Represents a FROM clause that is an ad-hoc table built with predefined values.
    /// </summary>
    public class AdHocTableFromClause : AbstractFrom
    {
        public ImmutableArray<string> Columns { get; set; }
        public ImmutableArray<object> Values { get; set; }
    }
}
