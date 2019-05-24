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
            var dictionary = BuildDictionaryOnUpdate(data);
            return AsUpdate(dictionary);
        }

        private Dictionary<string, object> BuildDictionaryOnUpdate(object data)
        {

            var dictionary = new Dictionary<string, object>();
            var props = data.GetType().GetRuntimeProperties();

            foreach (var property in props)
            {
                if (property.GetCustomAttribute(typeof(IgnoreAttribute)) != null)
                {
                    continue;
                }

                var value = property.GetValue(data);

                var colAttr = property.GetCustomAttributes<ColumnAttribute>();
                var isKey = false;
                var name = property.Name;

                foreach (var columnAttribute in colAttr)
                {
                    if (columnAttribute is KeyAttribute) isKey = true;
                    else name = columnAttribute.Name;
                }

                if (isKey)
                {
                    this.Where(name, value);
                    continue;
                }

                dictionary.Add(name, value);
            }

            return dictionary;
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

        public Query AsUpdate(IReadOnlyDictionary<string, object> data)
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
            });

            return this;
        }

    }
}