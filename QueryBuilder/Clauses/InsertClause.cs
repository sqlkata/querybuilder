using System.Collections.Generic;

namespace SqlKata
{
    /// <summary>
    ///     Represents an abstract class as base definition for an SQL inssert
    ///     clause
    /// </summary>
    public abstract class AbstractInsertClause : AbstractClause
    {
    }

    public class InsertClause : AbstractInsertClause
    {
        #region Properties
        public List<string> Columns { get; internal set; }

        public List<object> Values { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new InsertClause
            {
                Engine = Engine,
                Component = Component,
                Columns = Columns,
                Values = Values
            };
        }
        #endregion
    }

    public class InsertQueryClause : AbstractInsertClause
    {
        #region Properties
        public List<string> Columns { get; internal set; }

        public Query Query { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new InsertQueryClause
            {
                Engine = Engine,
                Component = Component,
                Columns = Columns,
                Query = Query.Clone()
            };
        }
        #endregion
    }
}