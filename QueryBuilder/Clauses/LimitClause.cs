namespace SqlKata
{
    public class LimitOffset : AbstractClause
    {
        #region Fields
        private int _limit;
        private int _offset;
        #endregion

        #region Properties
        /// <summary>
        ///     Gets or sets the limit
        /// </summary>
        public int Limit
        {
            get => _limit;
            set
            {
                if (value > 0)
                    _limit = value;
            }
        }

        /// <summary>
        ///     Gets or sets the offset
        /// </summary>
        public int Offset
        {
            get =>
                _offset;

            set
            {
                if (value > 0)
                    _offset = value;
            }
        }

        /// <summary>
        ///     Returns <c>true</c> when an limit has been set
        /// </summary>
        /// <returns></returns>
        public bool HasLimit()
        {
            return _limit > 0;
        }

        /// <summary>
        ///     Returns <c>true</c> when an offset has been set
        /// </summary>
        /// <returns></returns>
        public bool HasOffset()
        {
            return _offset > 0;
        }

        /// <summary>
        ///     Clears the limit only (reset to 0)
        /// </summary>
        /// <returns></returns>
        public LimitOffset ClearLimit()
        {
            _limit = 0;
            return this;
        }

        /// <summary>
        ///     Clears the offset only (resets to 0)
        /// </summary>
        /// <returns></returns>
        public LimitOffset ClearOffset()
        {
            _offset = 0;
            return this;
        }

        /// <summary>
        ///     Clears the limit and offset
        /// </summary>
        /// <returns></returns>
        public LimitOffset Clear()
        {
            return ClearLimit().ClearOffset();
        }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new LimitOffset
            {
                Engine = Engine,
                Offset = Offset,
                Limit = Limit,
                Component = Component
            };
        }
        #endregion
    }
}