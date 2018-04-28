using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsInsertGetId<T>(IEnumerable<string> columns, IEnumerable<object> values, string primaryKeyName = "id") where T : struct
        {

            if ((columns?.Count() ?? 0) == 0 || (values?.Count() ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and Values cannot be null or empty");
            }

            if (columns.Count() != values.Count())
            {
                throw new InvalidOperationException("Columns count should be equal to Values count");
            }

            Method = "insert_get_id";

            ClearComponent("insert_get_id").AddComponent("insert_get_id", new InsertClause
            {
                Columns = columns.ToList(),
                Values = values.Select(BackupNullValues).ToList(),
                PrimaryKeyName = primaryKeyName,
                PrimaryKeyType = typeof(T)
            });

            return this;
        }

        public Query AsInsertGetId<T>(IReadOnlyDictionary<string, object> data, string primaryKeyName = "id") where T : struct
        {
            if (data == null || data.Count == 0)
            {
                throw new InvalidOperationException("Values dictionary cannot be null or empty");
            }

            Method = "insert_get_id";

            ClearComponent("insert_get_id").AddComponent("insert_get_id", new InsertClause
            {
                Columns = data.Keys.ToList(),
                Values = data.Values.Select(BackupNullValues).ToList(),
                PrimaryKeyName = primaryKeyName,
                PrimaryKeyType = typeof(T)
            });

            return this;
        }
    }
}
