using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsAggregate(string type, string[] columns = null)
        {

            Method = "aggregate";

            AggregateClauses.Clear();
            AggregateClauses.Add(new AggregateClause
            {
                Type = type,
                Columns = columns?.ToList() ?? [],
                Engine = EngineScope
            });

            return this;
        }

        public Query AsCount(string[] columns = null)
        {
            var cols = columns?.ToList() ?? [];

            if (cols.Count == 0)
            {
                cols.Add("*");
            }

            return AsAggregate("count", [.. cols]);
        }

        public Query AsAvg(string column)
        {
            return AsAggregate("avg", [column]);
        }
        public Query AsAverage(string column)
        {
            return AsAvg(column);
        }

        public Query AsSum(string column)
        {
            return AsAggregate("sum", [column]);
        }

        public Query AsMax(string column)
        {
            return AsAggregate("max", [column]);
        }

        public Query AsMin(string column)
        {
            return AsAggregate("min", [column]);
        }
    }
}
