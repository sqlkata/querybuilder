using System.Collections.Immutable;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsAggregate(string type, string[]? columns = null)
        {
            Method = "aggregate";

            RemoveComponent("aggregate")
                .AddComponent(new AggregateClause
                {
                    Engine = EngineScope,
                    Component = "aggregate",
                    Type = type,
                    Columns = columns?.ToImmutableArray() ?? ImmutableArray<string>.Empty
                });

            return this;
        }

        public Query AsCount(string[] columns = null)
        {
            var cols = columns?.ToList() ?? new List<string>();

            if (!cols.Any()) cols.Add("*");

            return AsAggregate("count", cols.ToArray());
        }

        public Query AsAvg(string column)
        {
            return AsAggregate("avg", new[] { column });
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
