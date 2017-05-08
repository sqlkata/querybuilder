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
    }

    public class RawOrderBy : AbstractOrderBy, RawInterface
    {
        protected object[] _bindings;
        public string Expression { get; set; }
        public override object[] Bindings
        {
            get
            {
                return _bindings;
            }
            set
            {
                _bindings = value;
            }
        }
    }

    public class OrderByRandom : AbstractOrderBy
    {

    }

}