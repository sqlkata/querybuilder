using System;
using System.Collections.Concurrent;
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

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> CacheDictionaryProperties = new ConcurrentDictionary<Type, PropertyInfo[]>();

        private Dictionary<string, object> BuildDictionaryOnUpdate(object data)
        {
            var dictionary = new Dictionary<string, object>();
            var props = CacheDictionaryProperties.GetOrAdd(data.GetType(), type => type.GetRuntimeProperties().ToArray());

            foreach (PropertyInfo property in props)
            {
                if (property.GetCustomAttribute(typeof(IgnoreAttribute)) != null)
                {
                    continue;
                }

                var value = property.GetValue(data);

                var colAttr = property.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                var name = colAttr?.Name ?? property.Name;
                if (colAttr != null)
                {
                    if ((colAttr as KeyAttribute) != null)
                    {
                        this.Where(name, value);
                    }
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