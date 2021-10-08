namespace SqlKata
{
    public class LimitClause : AbstractClause
    {
        private int _limit;

        public int Limit
        {
            get => _limit;
            set => _limit = value > 0 ? value : _limit;
        }

        public bool HasLimit()
        {
            return _limit > 0;
        }

        public LimitClause Clear()
        {
            _limit = 0;
            return this;
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new LimitClause
            {
                Engine = Engine,
                Limit = Limit,
                Component = Component,
            };
        }
    }
}
