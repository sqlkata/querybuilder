using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {


        public Query Insert(IEnumerable<string> columns, IEnumerable<object> values)
        {
            if (columns.Count() != values.Count())
            {
                throw new InvalidOperationException("Columns count should be equal to Values count");
            }

            Method = "insert";

            Clear("insert").Add("insert", new InsertClause
            {
                Columns = columns.ToList(),
                Values = values.ToList()
            });

            return this;
        }

        public Query Insert(Dictionary<string, object> data)
        {

            Method = "insert";

            Clear("insert").Add("insert", new InsertClause
            {
                Columns = data.Keys.ToList(),
                Values = data.Values.ToList()
            });

            return this;
        }

        /// <summary>
        /// Produces insert from subquery
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public Query Insert(IEnumerable<string> columns, Query query)
        {

            Method = "insert";

            Clear("insert").Add("insert", new InsertQueryClause
            {
                Columns = columns.ToList(),
                Query = query
            });

            return this;
        }

    }
}