using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query Delete()
        {
            Method = "delete";
            return this;
        }

    }
}