using System;
using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractFrom : AbstractClause
    {
        /// <summary>
        /// Try to extract the Alias for the current clause.
        /// </summary>
        /// <returns></returns>
        public abstract string Alias { get; }
    }

    /// <summary>
    /// Represents a "from" clause.
    /// </summary>
    public class FromClause : AbstractFrom
    {
        public string Table { get; set; }

        public override string Alias
        {
            get
            {
                if (Table.ToLower().Contains(" as "))
                {
                    var segments = Table.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    return segments[2];
                }

                return Table;
            }
        }

        public override AbstractClause Clone()
        {
            return new FromClause
            {
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
        public Query Query { get; set; }
        public override object[] Bindings => Query.Bindings.ToArray();

        public override string Alias
        {
            get
            {
                return Query.QueryAlias;
            }
        }

        public override AbstractClause Clone()
        {
            return new QueryFromClause
            {
                Query = Query.Clone(),
                Component = Component,
            };
        }
    }

    public class RawFromClause : AbstractFrom, RawInterface
    {
        protected object[] _bindings;
        public string Expression { get; set; }
        public override object[] Bindings
        {
            get => _bindings;
            set
            {
                _bindings = value;
            }
        }

        public override string Alias => null;

        public override AbstractClause Clone()
        {
            return new RawFromClause
            {
                Expression = Expression,
                Bindings = _bindings,
                Component = Component,
            };
        }
    }

}