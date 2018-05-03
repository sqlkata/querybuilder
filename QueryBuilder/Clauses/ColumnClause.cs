using System;
using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractColumn : AbstractClause
    {
    }

    /// <summary>
    /// Represents "column" or "column as alias" clause.
    /// </summary>
    /// <seealso cref="SqlKata.AbstractColumn" />
    public class Column : AbstractColumn
    {
        /// <summary>
        /// Gets or sets the column name. Can be "columnName" or "columnName as columnAlias".
        /// </summary>
        /// <value>
        /// The column name.
        /// </value>
        public string Name { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new Column
            {
                Engine = Engine,
                Name = Name,
                Component = Component,
            };
        }
    }

    /// <summary>
    /// Represents column clause calculated using query.
    /// </summary>
    /// <seealso cref="SqlKata.AbstractColumn" />
    public class QueryColumn : AbstractColumn
    {
        /// <summary>
        /// Gets or sets the query that will be used for column value calculation.
        /// </summary>
        /// <value>
        /// The query for column value calculation.
        /// </value>
        public Query Query { get; set; }

        /// <inheritdoc />
        public override object[] GetBindings(string engine)
        {
            return Query.GetBindings(engine).ToArray();
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new QueryColumn
            {
                Engine = Engine,
                Query = Query.Clone(),
                Component = Component,
            };
        }
    }

    public class RawColumn : AbstractColumn, RawInterface
    {
        private object[] _bindings;

        /// <summary>
        /// Gets or sets the RAW expression.
        /// </summary>
        /// <value>
        /// The RAW expression.
        /// </value>
        public string Expression { get; set; }

        /// <summary>
        /// Sets bindings that used in current RawColumn clause.
        /// </summary>
        /// <value>
        /// The bindings.
        /// </value>
        public object[] Bindings { set => _bindings = value; }

        /// <inheritdoc />
        public override object[] GetBindings(string engine)
        {
            return _bindings;
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawColumn
            {
                Engine = Engine,
                Expression = Expression,
                _bindings = _bindings,
                Component = Component,
            };
        }
    }
}