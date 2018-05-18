namespace SqlKata
{
    public class LimitOffset : AbstractClause
    {
        private int _limit;
        private int _offset;

        public int Limit
        {
            get => _limit;

            set
            {
                if (value > 0)
                {
                    _limit = value;
                }
            }
        }

        public int Offset
        {
            get => _offset;

            set
            {
                if (value > 0)
                {
                    _offset = value;
                }
            }
        }

        public bool HasLimit()
        {
            return _limit > 0;
        }

        public bool HasOffset()
        {
            return _offset > 0;
        }

        public LimitOffset ClearLimit()
        {
            _limit = 0;
            return this;
        }

        public LimitOffset ClearOffset()
        {
            _offset = 0;
            return this;
        }

        public LimitOffset Clear()
        {
            return ClearLimit().ClearOffset();
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new LimitOffset
            {
                Engine = Engine,
                Offset = Offset,
                Limit = Limit,
                Component = Component,
            };
        }
    }
}