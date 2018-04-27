using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsInsertIncrement(IEnumerable<string> columns, IEnumerable<object> values)
        {

            if ((columns?.Count() ?? 0) == 0 || (values?.Count() ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and Values cannot be null or empty");
            }

            if (columns.Count() != values.Count())
            {
                throw new InvalidOperationException("Columns count should be equal to Values count");
            }

            Method = "insert_increment";

            ClearComponent("insert_increment").AddComponent("insert_increment", new InsertIncrementClause
            {
                Columns = columns.ToList(),
                Values = values.Select(BackupNullValues()).ToList(),
            });

            return this;
        }

        public Query AsInsertIncrement(IReadOnlyDictionary<string, object> data)
        {

            if (data == null || data.Count == 0)
            {
                throw new InvalidOperationException("Values dictionary cannot be null or empty");
            }

            Method = "insert_increment";

            ClearComponent("insert_increment").AddComponent("insert_increment", new InsertClause
            {
                Columns = data.Keys.ToList(),
                Values = data.Values.Select(BackupNullValues()).ToList()
            });

            return this;
        }
    }
}

