using System.Linq;

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

    public class RawCombine : AbstractCombine
    {
        public string Expression { get; set; }

        public object[] Bindings { get; set; }

        public override AbstractClause Clone()
        {
            return new RawCombine
            {
                Engine = Engine,
                Component = Component,
                Expression = Expression,
                Bindings = Bindings,
            };
        }
    }
}