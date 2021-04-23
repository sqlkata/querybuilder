using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsUpdate(object data)
        {
            var dictionary = BuildKeyValuePairsFromObject(data, true);

            return AsUpdate(dictionary);
        }

        public Query AsUpdate(IEnumerable<string> columns, IEnumerable<object> values)
        {
            var columnsList = columns.ToList();
            var valuesList = values.ToList();
            if ((bool) columnsList?.Any() == false || (bool) valuesList?.Any() == false)
            {
                throw new InvalidOperationException($"{columns} and {values} cannot be null or empty");
            }

            if (columnsList.Count() != valuesList.Count())
            {
                throw new InvalidOperationException($"{columns} count should be equal to {values} count");
            }

            Method = "update";

            ClearComponent("update").AddComponent("update", new InsertClause
            {
                Columns = columnsList.ToList(),
                Values = valuesList.ToList()
            });

            return this;
        }

        public Query AsUpdate(IEnumerable<KeyValuePair<string, object>> values)
        {
            var keyValuePairs = values.ToList();
            if (values == null || keyValuePairs.Any() == false)
            {
                throw new InvalidOperationException($"{values} cannot be null or empty");
            }

            Method = "update";

            ClearComponent("update").AddComponent("update", new InsertClause
            {
                Columns = keyValuePairs.Select(x => x.Key).ToList(),
                Values = keyValuePairs.Select(x => x.Value).ToList()
            });
            return this;
        }
    }
}
