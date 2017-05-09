using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractClause
    {
        public string Component { get; set; }
        public virtual object[] Bindings
        {
            get => new object[] { };
            set { }
        }

        public abstract AbstractClause Clone();
    }

}