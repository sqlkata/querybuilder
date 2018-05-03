using System;

namespace SqlKata
{
    public abstract class AbstractCombine : AbstractClause
    {

    }

    public class Combine : AbstractCombine
    {
        public Query Query { get; set; }
        public string Operation { get; set; }
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
        protected object[] _bindings;
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