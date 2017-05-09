using System;
using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractOrderBy : AbstractClause
    {

    }

    public class OrderBy : AbstractOrderBy
    {
        public string Column { get; set; }
        public bool Ascending { get; set; } = true;

        public override AbstractClause Clone()
        {
            return new OrderBy
            {
                Column = Column,
                Ascending = Ascending
            };
        }
    }

    public class RawOrderBy : AbstractOrderBy, RawInterface
    {
        protected object[] _bindings;
        public string Expression { get; set; }
        public override object[] Bindings
        {
            get => _bindings;
            set
            {
                _bindings = value;
            }
        }

        public override AbstractClause Clone()
        {
            return new RawOrderBy
            {
                Expression = Expression,
                Bindings = _bindings
            };
        }
    }

    public class OrderByRandom : AbstractOrderBy
    {
        public override AbstractClause Clone()
        {
            return new OrderByRandom();
        }
    }

}