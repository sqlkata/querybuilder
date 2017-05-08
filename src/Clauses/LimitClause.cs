using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public class LimitOffset : AbstractClause
    {
        private int _limit;
        private int _offset;

        public override object[] Bindings
        {
            get
            {
                return new[] { _limit, _offset }
                .Where(x => x > 0).Cast<object>().ToArray();
            }
        }

        public int Limit
        {
            get
            {
                return _limit;
            }

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
            get
            {
                return _offset;
            }

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
    }
}