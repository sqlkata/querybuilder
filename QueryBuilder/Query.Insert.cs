using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsInsert(object data, bool returnId = false)
        {
            var propertiesKeyValues = BuildKeyValuePairsFromObject(data);

            return AsInsert(propertiesKeyValues, returnId);
        }

        public Query AsInsert(IEnumerable<string> columns, IEnumerable<object> values)
        {
            var columnsList = columns?.ToList();
            var valuesList = values?.ToList();

            if ((columnsList?.Count ?? 0) == 0 || (valuesList?.Count ?? 0) == 0)
            {
                throw new InvalidOperationException($"{nameof(columns)} and {nameof(values)} cannot be null or empty");
            }

            if (columnsList.Count != valuesList.Count)
            {
                throw new InvalidOperationException($"{nameof(columns)} and {nameof(values)} cannot be null or empty");
            }

            Method = "insert";

            ClearComponent("insert").AddComponent("insert", new InsertClause
            {
                Columns = columnsList,
                Values = valuesList
            });

            return this;
        }

        public Query AsInsert(IEnumerable<KeyValuePair<string, object>> values, bool returnId = false)
        {
            var valuesList = values.ToList();
            if (values == null || valuesList.Any() == false)
            {
                throw new InvalidOperationException($"{values} argument cannot be null or empty");
            }

            Method = "insert";

            ClearComponent("insert").AddComponent("insert", new InsertClause
            {
                Columns = valuesList.Select(x=>x.Key).ToList(),
                Values = valuesList.Select(x => x.Value).ToList(),
                ReturnId = returnId
            });

            return this;
        }

        /// <summary>
        /// Produces insert multi records
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="rowsValues"></param>
        /// <returns></returns>
        public Query AsInsert(IEnumerable<string> columns, IEnumerable<IEnumerable<object>> rowsValues)
        {
            var columnsList = columns?.ToList();
            var valuesCollectionList = rowsValues?.ToList();

            if ((columnsList?.Count ?? 0) == 0 || (valuesCollectionList?.Count ?? 0) == 0)
            {
                throw new InvalidOperationException($"{nameof(columns)} and {nameof(rowsValues)} cannot be null or empty");
            }

            Method = "insert";

            ClearComponent("insert");

            foreach (var valuesList in valuesCollectionList.Select(values => values.ToList()))
            {
                if (columnsList.Count != valuesList.Count)
                {
                    throw new InvalidOperationException($"{nameof(columns)} count should be equal to each {nameof(rowsValues)} entry count");
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
        /// Produces insert from sub-query
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
                Query = query.Clone()
            });

            return this;
        }
    }
}
