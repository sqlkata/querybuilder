using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsInsert(object data, bool returnId = false)
        {
            var dictionary = new Dictionary<string, object>();

            var props = data.GetType().GetRuntimeProperties();

            foreach (var item in props)
            {
                dictionary.Add(item.Name, item.GetValue(data));
            }

            return AsInsert(dictionary, returnId);
        }

        public Query AsInsert(IEnumerable<string> columns, IEnumerable<object> values)
        {
            var columnsList = columns?.ToList();
            var valuesList = values?.ToList();

            if ((columnsList?.Count ?? 0) == 0 || (valuesList?.Count ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and Values cannot be null or empty");
            }

            if (columnsList.Count != valuesList.Count)
            {
                throw new InvalidOperationException("Columns count should be equal to Values count");
            }

            Method = "insert";

            ClearComponent("insert").AddComponent("insert", new InsertClause
            {
                Columns = columnsList,
                Values = valuesList
            });

            return this;
        }

        public Query AsInsert(IReadOnlyDictionary<string, object> data, bool returnId = false)
        {
            if (data == null || data.Count == 0)
            {
                throw new InvalidOperationException("Values dictionary cannot be null or empty");
            }

            Method = "insert";

            ClearComponent("insert").AddComponent("insert", new InsertClause
            {
                Columns = data.Keys.ToList(),
                Values = data.Values.ToList(),
                ReturnId = returnId,
            });

            return this;
        }

        /// <summary>
        /// Produces insert multi records
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="valuesCollection"></param>
        /// <returns></returns>
        public Query AsInsert(IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection)
        {
            var columnsList = columns?.ToList();
            var valuesCollectionList = valuesCollection?.ToList();

            if ((columnsList?.Count ?? 0) == 0 || (valuesCollectionList?.Count ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and valuesCollection cannot be null or empty");
            }

            Method = "insert";

            ClearComponent("insert");

            foreach (var values in valuesCollectionList)
            {
                var valuesList = values.ToList();
                if (columnsList.Count != valuesList.Count)
                {
                    throw new InvalidOperationException("Columns count should be equal to each Values count");
                }

                AddComponent("insert", new InsertClause
                {
                    Columns = columnsList,
                    Values = valuesList
                });
            }

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
                Query = query.Clone(),
            });

            return this;
        }

    }
}