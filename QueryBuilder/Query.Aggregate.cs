using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        /**********************************************************************
         ** Generic aggregate                                                **
         **********************************************************************/
        public Query SelectAggregate(string type, IEnumerable<string> columns, string alias = null)
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
        public Query SelectCount(string column = null, string alias = null)
        {
            return SelectCount(column != null ? new[] { column } : new string[] { }, alias);
        }

        public Query SelectCount(IEnumerable<string> columns, string alias = null)
        {
            return SelectAggregate("count", columns.Count() == 0 ? new[] { "*" } : columns, alias);
        }


        /**********************************************************************
         ** Average                                                          **
         **********************************************************************/
        public Query SelectAvg(string column, string alias = null)
        {
            return SelectAggregate("avg", new[] { column }, alias);
        }

        public Query SelectAverage(string column, string alias = null)
        {
            return SelectAvg(column, alias);
        }


        /**********************************************************************
         ** Sum                                                              **
         **********************************************************************/
        public Query SelectSum(string column, string alias = null)
        {
            return SelectAggregate("sum", new[] { column }, alias);
        }


        /**********************************************************************
         ** Maximum                                                          **
         **********************************************************************/
        public Query SelectMax(string column, string alias = null)
        {
            return SelectAggregate("max", new[] { column }, alias);
        }


        /**********************************************************************
         ** Minimum                                                          **
         **********************************************************************/
        public Query SelectMin(string column, string alias = null)
        {
            return SelectAggregate("min", new[] { column }, alias);
        }
    }
}
