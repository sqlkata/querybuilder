using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        /**********************************************************************
         ** Generic aggregate                                                **
         **********************************************************************/
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

        /**********************************************************************
         ** Count                                                            **
         **********************************************************************/
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


        /**********************************************************************
         ** Average                                                          **
         **********************************************************************/
        public Query AsAvg(string column) =>
            AsAverage(column);

        public Query AsAverage(string column) =>
            AsAggregate("avg", new[] { column }, null);

        public Query AsAvgAs(string column, string alias) =>
            AsAverageAs(column, alias);

        public Query AsAverageAs(string column, string alias) =>
            AsAggregate("avg", new[] { column }, alias);


        /**********************************************************************
         ** Sum                                                              **
         **********************************************************************/
        public Query AsSum(string column) =>
            AsAggregate("sum", new[] { column }, null);

        public Query AsSumAs(string column, string alias = null) =>
            AsAggregate("sum", new[] { column }, alias);


        /**********************************************************************
         ** Maximum                                                          **
         **********************************************************************/
        public Query AsMax(string column) =>
            AsAggregate("max", new[] { column }, null);

        public Query AsMaxAs(string column, string alias = null) =>
            AsAggregate("max", new[] { column }, alias);


        /**********************************************************************
         ** Minimum                                                          **
         **********************************************************************/
        public Query AsMin(string column) =>
            AsAggregate("min", new[] { column }, null);

        public Query AsMinAs(string column, string alias = null) =>
            AsAggregate("min", new[] { column }, alias);
    }
}
