using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {


        public Query AsInsert(IEnumerable<string> columns, IEnumerable<object> values)
        {

            if ((columns?.Count() ?? 0) == 0 || (values?.Count() ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and Values cannot be null or empty");
            }

            if (columns.Count() != values.Count())
            {
                throw new InvalidOperationException("Columns count should be equal to Values count");
            }

            Method = "insert";

            ClearComponent("insert").AddComponent("insert", new InsertClause
            {
                Columns = columns.ToList(),
                Values = values.Select(this.BackupNullValues()).ToList()
            });

            return this;
        }

        public Query AsInsert(IReadOnlyDictionary<string, object> data)
        {

            if (data == null || data.Count == 0)
            {
                throw new InvalidOperationException("Values dictionary cannot be null or empty");
            }

            Method = "insert";

            ClearComponent("insert").AddComponent("insert", new InsertClause
            {
                Columns = data.Keys.ToList(),
                Values = data.Values.Select(this.BackupNullValues()).ToList()
            });

            return this;
        }

        /// <summary>
        /// Produces insert from subquery
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public Query AsInsert(IEnumerable<string> columns, Query query)
        {

            Method = "insert";

            ClearComponent("insert").AddComponent("insert", new InsertQueryClause
            {
                Columns = columns.ToList(),
                Query = query
            });

            return this;
        }

    }
}