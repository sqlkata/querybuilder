using System.Linq;
using SqlKata.Interfaces;

namespace SqlKata
{
    public partial class Query
    {
        public IQuery AsAggregate(string type, params string[] columns)
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

        public IQuery AsCount(params string[] columns)
        {
            var cols = columns.ToList();

            if (!cols.Any())
            {
                cols.Add("*");
            }

            return AsAggregate("count", cols.ToArray());
        }

        public IQuery AsAvg(string column)
        {
            return AsAggregate("avg", column);
        }
        public IQuery AsAverage(string column)
        {
            return AsAvg(column);
        }

        public IQuery AsSum(string column)
        {
            return AsAggregate("sum", column);
        }

        public IQuery AsMax(string column)
        {
            return AsAggregate("max", column);
        }

        public IQuery AsMin(string column)
        {
            return AsAggregate("min", column);
        }
    }
}