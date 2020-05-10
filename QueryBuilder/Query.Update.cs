using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata
{

    public partial class Query
    {

        public Query AsUpdate(object data, IEnumerable<string> returnColumns = null)
        {
            var dictionary = BuildDictionaryFromObject(data, considerKeys: true);

            return AsUpdate(dictionary, returnColumns);
        }

        public Query AsUpdate(IEnumerable<string> columns, IEnumerable<object> values, IEnumerable<string> returnColumns = null)
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
                Values = values.ToList(),
                ReturnColumns = returnColumns?.ToList(),
            });

            return this;
        }

        public Query AsUpdate(IReadOnlyDictionary<string, object> data, IEnumerable<string> returnColumns = null)
        {

            if (data == null || data.Count == 0)
            {
                throw new InvalidOperationException("Values dictionary cannot be null or empty");
            }

            Method = "update";

            ClearComponent("update").AddComponent("update", new InsertClause
            {
                Columns = data.Keys.ToList(),
                Values = data.Values.ToList(),
                ReturnColumns = returnColumns?.ToList(),
            });

            return this;
        }

    }
}