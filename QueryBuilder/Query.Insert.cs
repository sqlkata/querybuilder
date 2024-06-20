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
            if (values == null || values.Any() == false)
            {
                throw new InvalidOperationException($"{values} argument cannot be null or empty");
            }

            Method = "insert";

            ClearComponent("insert").AddComponent("insert", new InsertClause
            {
                Columns = values.Select(x => x.Key).ToList(),
                Values = values.Select(x => x.Value).ToList(),
                ReturnId = returnId,
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

            foreach (var values in valuesCollectionList)
            {
                var valuesList = values.ToList();
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

        public Query AsInsert(IEnumerable<IEnumerable<KeyValuePair<string, object>>> values)
        {
            if (values == null || !values.Any())
            {
                throw new InvalidOperationException($"{values} argument cannot be null or empty");
            }

            var columnsList = values.First().Select(x => x.Key).OrderBy(x => x).ToList();
            if (!columnsList.Any())
            {
                throw new InvalidOperationException($"Elements in {values} argument cannot be empty");
            }

            var rowsValuesList = new List<List<object>>();

            foreach (var rowValues in values)
            {
                int rowValuesCount = rowValues.Count();
                if (rowValuesCount != columnsList.Count())
                {
                    throw new InvalidOperationException($"Not all elements in {values} contain the same columns.");
                }

                var valuesList = new List<object>();
                var sortedRowValuesList = rowValues.OrderBy(x => x.Key).ToList();
                for (int i = 0; i < rowValuesCount; i++)
                {
                    if (columnsList[i] != sortedRowValuesList[i].Key)
                    {
                        throw new InvalidOperationException($"Not all elements in {values} contain the same columns.");
                    }

                    valuesList.Add(sortedRowValuesList[i].Value);
                }

                rowsValuesList.Add(valuesList);
            }

            return AsInsert(columnsList, rowsValuesList);
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
