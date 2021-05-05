namespace SqlKata
{
    public class OffsetClause : AbstractClause
    {
        private int _offset;

        public int Offset
        {
            get => _offset;
            set => _offset = value > 0 ? value : _offset;
        }

        public bool HasOffset()
        {
            return _offset > 0;
        }

        public OffsetClause Clear()
        {
            _offset = 0;
            return this;
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new OffsetClause
            {
                Engine = Engine,
                Offset = Offset,
                Component = Component,
            };
        }
    }
}
