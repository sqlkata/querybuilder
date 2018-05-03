using System;

namespace SqlKata
{
    public abstract class AbstractCombine : AbstractClause
    {

    }

    public class Combine : AbstractCombine
    {
        /// <summary>
        /// Gets or sets the query to be combined with.
        /// </summary>
        /// <value>
        /// The query that will be combined.
        /// </value>
        public Query Query { get; set; }

        /// <summary>
        /// Gets or sets the combine operation, e.g. "UNION", etc.
        /// </summary>
        /// <value>
        /// The combine operation.
        /// </value>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Combine"/> clause will combine all.
        /// </summary>
        /// <value>
        ///   <c>true</c> if all; otherwise, <c>false</c>.
        /// </value>
        public bool All { get; set; } = false;

        /// <inheritdoc />
        public override object[] GetBindings(string engine)
        {
            return Query.GetBindings(engine).ToArray();
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new Combine
            {
                Engine = Engine,
                Operation = Operation,
                Component = Component,
                Query = Query,
                All = All,
            };
        }
    }

    public class RawCombine : AbstractCombine, RawInterface
    {
        private object[] _bindings;

        public string Expression { get; set; }

        public object[] Bindings { set => _bindings = value; }

        /// <inheritdoc />
        public override object[] GetBindings(string engine)
        {
            return _bindings;
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawCombine
            {
                Engine = Engine,
                Component = Component,
                Expression = Expression,
                _bindings = _bindings
            };
        }
    }
}