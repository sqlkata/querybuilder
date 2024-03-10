using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SqlKata.Extensions;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsUpdate(object data)
        {
            var dictionary = BuildKeyValuePairsFromObject(data, considerKeys: true);

            return AsUpdate(dictionary);
        }

        public Query AsUpdate(IEnumerable<string> columns, IEnumerable<object> values)
        {
            var columnsList = columns?.ToList();
            var valuesList = values?.ToList();

            if ((columnsList?.Count ?? 0) == 0 || (valuesList?.Count ?? 0) == 0)
            {
                throw new InvalidOperationException($"{columns} and {values} cannot be null or empty");
            }

            if (columnsList.Count != valuesList.Count)
            {
                throw new InvalidOperationException($"{columns} count should be equal to {values} count");
            }

            Method = "update";

            ClearComponent("update").AddComponent("update", new InsertClause
            {
                Data = columnsList.MergeKeysAndValues(valuesList)
            });

            return this;
        }

        public Query AsUpdate(IEnumerable<KeyValuePair<string, object>> values)
        {
            if (values == null || values.Any() == false)
            {
                throw new InvalidOperationException($"{values} cannot be null or empty");
            }

            Method = "update";

            ClearComponent("update").AddComponent("update", new InsertClause
            {
                Data = values.CreateDictionary()
            });

            return this;
        }

        public Query AsIncrement(string column, int value = 1)
        {
            Method = "update";
            AddOrReplaceComponent("update", new IncrementClause
            {
                Column = column,
                Value = value
            });

            return this;
        }

        public Query AsDecrement(string column, int value = 1)
        {
            return AsIncrement(column, -value);
        }
    }
}
