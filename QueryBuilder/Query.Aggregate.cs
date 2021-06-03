using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsAggregate(string type, string[] columns = null)
        {
            return AggregateAs(
                type,
                columns ?? new string[] { },
                null // old interface always uses 'type' as alias name
            );
        }

        public Query AggregateAs(string type, IEnumerable<string> columns, string alias)
        {

            Method = "aggregate";

            this.ClearComponent("aggregate")
            .AddComponent("aggregate", new AggregateClause
            {
                Type = type,
                Columns = columns.ToList(),
                Alias = alias
            });

            return this;
        }

        public Query CountAs(string column = null, string alias = null)
        {
            return CountAs(new[] { column ?? "*" }, alias);
        }

        public Query CountAs(IEnumerable<string> columns, string alias = null)
        {
            return AggregateAs("count", columns, alias);
        }

        public Query AsAvg(string column)
        {
            return AsAggregate("avg", new string[] { column });
        }
        public Query AsAverage(string column)
        {
            return AsAvg(column);
        }

        public Query AsSum(string column)
        {
            return AsAggregate("sum", new[] { column });
        }

        public Query AsMax(string column)
        {
            return AsAggregate("max", new[] { column });
        }

        public Query AsMin(string column)
        {
            return AsAggregate("min", new[] { column });
        }
    }
}
