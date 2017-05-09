using System.Collections.Generic;

namespace SqlKata
{
    public class BaseJoin : AbstractClause
    {
        public Join Join { get; set; }
        public override object[] Bindings
        {
            get
            {
                return Join.Bindings.ToArray();
            }
        }
    }

}