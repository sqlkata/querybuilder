using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsAggregate(string type, IEnumerable<string> columns, string alias = null)
        {
            if (columns.Count() == 0)
            {
                throw new System.ArgumentException("Cannot aggregate without columns");
            }

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

        public Query AsCount(params string[] columns)
        {
            var cols = columns.ToList();

            if (!cols.Any())
            {
                cols.Add("*");
            }

            return AsAggregate("count", cols.ToArray());
        }

        public Query AsCountAs(string column, string alias) =>
            AsAggregate("count", new string[] { column }, alias);

        public Query AsCountAs(IEnumerable<string> columns, string alias) =>
            AsAggregate("count", columns, alias);

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
