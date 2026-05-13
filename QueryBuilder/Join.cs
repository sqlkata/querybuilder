using System;

namespace SqlKata
{
    /// <summary>
    /// Builds a SQL join clause and its join conditions.
    /// </summary>
    public class Join : BaseQuery<Join>
    {
        protected string _type = "inner join";

        /// <summary>
        /// Gets or sets the SQL join type.
        /// </summary>
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value.ToUpperInvariant();
            }
        }

        /// <summary>
        /// Initializes a new inner join clause.
        /// </summary>
        public Join() : base()
        {
        }

        /// <summary>
        /// Creates a copy of the join clause.
        /// </summary>
        /// <returns>A cloned join clause.</returns>
        public override Join Clone()
        {
            var clone = base.Clone();
            clone._type = _type;
            return clone;
        }

        /// <summary>
        /// Sets the SQL join type.
        /// </summary>
        /// <param name="type">The join type, for example <c>left join</c>.</param>
        /// <returns>The current join instance.</returns>
        public Join AsType(string type)
        {
            Type = type;
            return this;
        }

        /// <summary>
        /// Alias for "from" operator.
        /// Since "from" does not sound well with join clauses
        /// </summary>
        /// <param name="table">The table to join with.</param>
        /// <returns>The current join instance.</returns>
        public Join JoinWith(string table) => From(table);

        /// <summary>
        /// Adds a subquery as the joined source.
        /// </summary>
        /// <param name="query">The subquery to join with.</param>
        /// <returns>The current join instance.</returns>
        public Join JoinWith(Query query) => From(query);

        /// <summary>
        /// Adds a callback-created subquery as the joined source.
        /// </summary>
        /// <param name="callback">A callback that configures the joined query.</param>
        /// <returns>The current join instance.</returns>
        public Join JoinWith(Func<Query, Query> callback) => From(callback);

        /// <summary>
        /// Sets the join type to <c>inner join</c>.
        /// </summary>
        /// <returns>The current join instance.</returns>
        public Join AsInner() => AsType("inner join");

        /// <summary>
        /// Sets the join type to <c>outer join</c>.
        /// </summary>
        /// <returns>The current join instance.</returns>
        public Join AsOuter() => AsType("outer join");

        /// <summary>
        /// Sets the join type to <c>left join</c>.
        /// </summary>
        /// <returns>The current join instance.</returns>
        public Join AsLeft() => AsType("left join");

        /// <summary>
        /// Sets the join type to <c>right join</c>.
        /// </summary>
        /// <returns>The current join instance.</returns>
        public Join AsRight() => AsType("right join");

        /// <summary>
        /// Sets the join type to <c>cross join</c>.
        /// </summary>
        /// <returns>The current join instance.</returns>
        public Join AsCross() => AsType("cross join");

        /// <summary>
        /// Adds a join condition comparing two columns.
        /// </summary>
        /// <param name="first">The first column name.</param>
        /// <param name="second">The second column name.</param>
        /// <param name="op">The comparison operator.</param>
        /// <returns>The current join instance.</returns>
        public Join On(string first, string second, string op = "=")
        {
            return AddComponent("where", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot()
            });

        }

        /// <summary>
        /// Adds an OR join condition comparing two columns.
        /// </summary>
        /// <param name="first">The first column name.</param>
        /// <param name="second">The second column name.</param>
        /// <param name="op">The comparison operator.</param>
        /// <returns>The current join instance.</returns>
        public Join OrOn(string first, string second, string op = "=")
        {
            return Or().On(first, second, op);
        }

        /// <summary>
        /// Creates a new join query instance.
        /// </summary>
        /// <returns>A new join instance.</returns>
        public override Join NewQuery()
        {
            return new Join();
        }
    }
}
