using System;

namespace SqlKata
{
    public abstract class AbstractFrom : AbstractClause
    {
        protected string _alias;

        /// <summary>
        /// Table hints
        /// </summary>
        public string[] Hints { get; internal set; }

        /// <summary>
        /// Try to extract the Alias for the current clause.
        /// </summary>
        /// <returns></returns>
        public virtual string Alias { get => _alias; set => _alias = value; }
    }

    /// <summary>
    /// Represents a "from" clause.
    /// </summary>
    public class FromClause : AbstractFrom
    {
        /// <summary>
        /// The table name
        /// </summary>
        public string Table { get; internal set; }

        public override string Alias
        {
            get
            {
                if (Table.IndexOf(" as ", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var segments = Table.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    return segments[2];
                }

                return Table;
            }
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new FromClause
            {
                Alias = Alias,
                Table = Table,
                Component = Component,
            };
        }
    }

    /// <summary>
    /// Represents a "from subquery" clause.
    /// </summary>
    public class QueryFromClause : AbstractFrom
    {
        public Query Query { get; internal set; }

        public override string Alias => string.IsNullOrEmpty(_alias) ? Query.QueryAlias : _alias;

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new QueryFromClause
            {
                Engine = Engine,
                Alias = Alias,
                Query = Query.Clone(),
                Component = Component,
            };
        }
    }

    public class RawFromClause : AbstractFrom, IRaw
    {
        public string Expression { get; internal set; }
        public object[] Bindings { internal set; get; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawFromClause
            {
                Engine = Engine,
                Alias = Alias,
                Expression = Expression,
                Bindings = Bindings,
                Component = Component,
            };
        }
    }
}