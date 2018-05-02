using System.Collections.Generic;

namespace SqlKata
{
    public class AggregateClause : AbstractClause
    {
        public List<string> Columns { get; set; }
        public string Type { get; set; }

        public override AbstractClause Clone()
        {
            return new AggregateClause
            {
                Engine = Engine,
                Type = Type,
                Columns = new List<string>(Columns),
                Component = Component,
            };
        }
    }
}