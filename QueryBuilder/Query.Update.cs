using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

            if ((columns?.Count() ?? 0) == 0 || (values?.Count() ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and Values cannot be null or empty");
            }

            if (columns.Count() != values.Count())
            {
                throw new InvalidOperationException("Columns count should be equal to Values count");
            }

            Method = "update";

            ClearComponent("update").AddComponent("update", new InsertClause
            {
                Columns = columns.ToList(),
                Values = values.ToList()
            });

            return this;
        }

        public Query AsUpdate(IEnumerable<KeyValuePair<string, object>> data)
        {

            if (data == null || data.Any() == false)
            {
                throw new InvalidOperationException("Values dictionary cannot be null or empty");
            }

            Method = "update";

            ClearComponent("update").AddComponent("update", new InsertClause
            {
                Columns = data.Select(x => x.Key).ToList(),
                Values = data.Select(x => x.Value).ToList(),
            });

            return this;
        }

    }
}