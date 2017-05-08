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

            (GetOne("limit") as LimitOffset)?.Clear();

            Clear("select")
            .Clear("group")
            .Clear("order")
            .Clear("aggregate")
            .Add("aggregate", new AggregateClause
            {
                Type = type,
                Columns = columns.ToList()
            });

            return this;
        }

        public Query Count(params string[] columns)
        {
            var cols = columns.ToList();

            if (!cols.Any())
            {
                cols.Add("*");
            }

            return Aggregate("count", cols.ToArray());
        }

        public Query Avg(string column)
        {
            return Aggregate("avg", column);
        }
        public Query Average(string column)
        {
            return Avg(column);
        }

        public Query Sum(string column)
        {
            return Aggregate("sum", column);
        }

        public Query Max(string column)
        {
            return Aggregate("max", column);
        }

        public Query Min(string column)
        {
            return Aggregate("min", column);
        }
    }
}