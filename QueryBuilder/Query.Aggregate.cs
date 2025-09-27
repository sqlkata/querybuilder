namespace SqlKata;

using System.Collections.Generic;
using System.Linq;

public partial class Query {
    public Query AsAggregate(string type, string[] columns = null) {

        Method = "aggregate";

        this.ClearComponent("aggregate")
        .AddComponent("aggregate", new AggregateClause {
            Type = type,
            Columns = columns?.ToList() ?? [],
        });

        return this;
    }

    public Query AsCount(string[] columns = null) {
        var cols = columns?.ToList() ?? [];

        if (!cols.Any()) {
            cols.Add("*");
        }

        return AsAggregate("count", cols.ToArray());
    }

    public Query AsAvg(string column) {
        return AsAggregate("avg", new string[] { column });
    }
    public Query AsAverage(string column) {
        return AsAvg(column);
    }

    public Query AsSum(string column) {
        return AsAggregate("sum", new[] { column });
    }

    public Query AsMax(string column) {
        return AsAggregate("max", new[] { column });
    }

    public Query AsMin(string column) {
        return AsAggregate("min", new[] { column });
    }
}
