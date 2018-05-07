using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractClause
    {
        public string Engine { get; set; } = null;
        public string Component { get; set; }
        public abstract AbstractClause Clone();
    }

}