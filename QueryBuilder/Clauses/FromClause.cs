using System;
using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractFrom : AbstractClause
    {
        protected string _alias;
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
        public Query Query { get; set; }
        public override object[] GetBindings(string engine)
        {
            return Query.GetBindings(engine).ToArray();
        }

        public override string Alias
        {
            get
            {
                return string.IsNullOrEmpty(_alias) ? Query.QueryAlias : _alias;
            }
        }

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
        public string Expression { get; set; }
        protected object[] _bindings;
        public object[] Bindings { set => _bindings = value; }
        public override object[] GetBindings(string engine)
        {
            return _bindings;
        }

        public override AbstractClause Clone()
        {
            return new RawFromClause
            {
                Engine = Engine,
                Alias = Alias,
                Expression = Expression,
                _bindings = _bindings,
                Component = Component,
            };
        }
    }

}