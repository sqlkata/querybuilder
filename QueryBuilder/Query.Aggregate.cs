using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query Aggregate(string type, params string[] columns)
        {

            // Clear up the following statements since they are not needed in 
            // case of aggregation

            (GetOneComponent("limit") as LimitOffset)?.Clear();

            ClearComponent("select")
            .ClearComponent("group")
            .ClearComponent("order")
            .ClearComponent("aggregate")
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

            return Aggregate("count", cols.ToArray());
        }

        public Query AsAvg(string column)
        {
            return Aggregate("avg", column);
        }
        public Query AsAverage(string column)
        {
            return AsAvg(column);
        }

        public Query AsSum(string column)
        {
            return Aggregate("sum", column);
        }

        public Query AsMax(string column)
        {
            return Aggregate("max", column);
        }

        public Query AsMin(string column)
        {
            return Aggregate("min", column);
        }
    }
}