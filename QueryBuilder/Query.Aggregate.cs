using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        /**********************************************************************
         ** Generic aggregate                                                **
         **********************************************************************/
        public Query AggregateAs(string type, IEnumerable<string> columns, string alias = null)
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


        /**********************************************************************
         ** Count                                                            **
         **********************************************************************/
        public Query CountAs(string column = null, string alias = null)
        {
            return CountAs(column != null ? new[] { column } : new string[] { }, alias);
        }

        public Query CountAs(IEnumerable<string> columns, string alias = null)
        {
            return AggregateAs("count", columns.Count() == 0 ? new[] { "*" } : columns, alias);
        }


        /**********************************************************************
         ** Average                                                          **
         **********************************************************************/
        public Query AvgAs(string column, string alias = null)
        {
            return AggregateAs("avg", new[] { column }, alias);
        }

        public Query AverageAs(string column, string alias = null)
        {
            return AvgAs(column, alias);
        }


        /**********************************************************************
         ** Sum                                                              **
         **********************************************************************/
        public Query SumAs(string column, string alias = null)
        {
            return AggregateAs("sum", new[] { column }, alias);
        }


        /**********************************************************************
         ** Maximum                                                          **
         **********************************************************************/
        public Query MaxAs(string column, string alias = null)
        {
            return AggregateAs("max", new[] { column }, alias);
        }


        /**********************************************************************
         ** Minimum                                                          **
         **********************************************************************/
        public Query MinAs(string column, string alias = null)
        {
            return AggregateAs("min", new[] { column }, alias);
        }
    }
}
