using System;

namespace SqlKata
{
    /// <summary>
    ///     Represents an abstract class as base definition for an SQL from clause
    /// </summary>
    public abstract class AbstractFrom : AbstractClause
    {
        #region Properties
        /// <summary>
        ///     Returns the hints to use on a table
        /// </summary>
        public string[] Hints { get; internal set; }

        /// <summary>
        ///     Try to extract the Alias for the current clause.
        /// </summary>
        /// <returns></returns>
        public virtual string Alias { get; set; }
        #endregion
    }

    /// <inheritdoc />
    public class FromClause : AbstractFrom
    {
        #region Properties
        /// <summary>
        ///     Returns the table name
        /// </summary>
        public string Table { get; internal set; }

        /// <summary>
        /// Returns the alias for the table
        /// </summary>
        public override string Alias
        {
            get
            {
                if (Table.IndexOf(" as ", StringComparison.OrdinalIgnoreCase) < 0) return Table;
                var segments = Table.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                return segments[2];
            }
        }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new FromClause
            {
                Alias = Alias,
                Table = Table,
                Component = Component
            };
        }
        #endregion
    }

    /// <inheritdoc />
    public class QueryFromClause : AbstractFrom
    {
        #region Properties
        /// <inheritdoc />
        public Query Query { get; internal set; }

        /// <inheritdoc />
        public override string Alias => string.IsNullOrEmpty(base.Alias) ? Query.QueryAlias : base.Alias;
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new QueryFromClause
            {
                Engine = Engine,
                Alias = Alias,
                Query = Query.Clone(),
                Component = Component
            };
        }
        #endregion
    }
    
    /// <summary>
    ///     Represents a "from" clause in it's RAW form 
    ///     with it's own expression and bindings
    /// </summary>
    public class RawFromClause : AbstractFrom, IRaw
    {
        #region Properties
        /// <inheritdoc />
        public string Expression { get; internal set; }

        /// <inheritdoc />
        public object[] Bindings { internal set; get; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawFromClause
            {
                Engine = Engine,
                Alias = Alias,
                Expression = Expression,
                Bindings = Bindings,
                Component = Component
            };
        }
        #endregion
    }
}