using System;
using System.Collections.Generic;

namespace SqlKata
{
    public class AggregateClause : AbstractClause
    {
        public List<string> Columns { get; set; }
        public string Type { get; set; }
    }
}