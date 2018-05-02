using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsAggregate(string type, params string[] columns)
        {

            Method = "aggregate";

            this.ClearComponent("aggregate")
            .AddComponent("aggregate", new AggregateClause
            {
                Type = type,
                Columns = columns.ToList()
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

        public Query AsAvg(string column)
        {
            return AsAggregate("avg", column);
        }
        public Query AsAverage(string column)
        {
            return AsAvg(column);
        }

        public Query AsSum(string column)
        {
            return AsAggregate("sum", column);
        }

        public Query AsMax(string column)
        {
            return AsAggregate("max", column);
        }

        public Query AsMin(string column)
        {
            return AsAggregate("min", column);
        }
    }
}