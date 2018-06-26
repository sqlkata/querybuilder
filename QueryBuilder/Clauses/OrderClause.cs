namespace SqlKata
{
    public abstract class AbstractOrderBy : AbstractClause
    {
    }

    public class OrderBy : AbstractOrderBy
    {
        public string Column { get; internal set; }
        public bool Ascending { get; internal set; } = true;

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new OrderBy
            {
                Engine = Engine,
                Component = Component,
                Column = Column,
                Ascending = Ascending
            };
        }
    }

    public class OrderByRandom : AbstractOrderBy
    {
        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new OrderByRandom
            {
                Engine = Engine
            };
        }
        #endregion
    }

    /// <summary>
    ///     Represents an "order by" in it's RAW form with 
    ///     it's own expression and bindings
    /// </summary>
    public class RawOrderBy : AbstractOrderBy, IRaw
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
            return new RawOrderBy
            {
                Engine = Engine,
                Component = Component,
                Expression = Expression,
                Bindings = Bindings
            };
        }
        #endregion
    }
}