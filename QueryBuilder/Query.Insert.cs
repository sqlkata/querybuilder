using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsInsert(object data, bool returnId = false, IEnumerable<string> returnColumns = null)
        {
            var dictionary = BuildDictionaryFromObject(data);

            return AsInsert(dictionary, returnId, returnColumns);
        }

        public Query AsInsert(object data, IEnumerable<string> returnColumns)
        {
            return AsInsert(data, false, returnColumns);
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

        public Query AsInsert(IReadOnlyDictionary<string, object> data, bool returnId = false, IEnumerable<string> returnColumns = null)
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
                ReturnColumns = returnColumns?.ToList(),
            });

            return this;
        }

        public Query AsInsert(IReadOnlyDictionary<string, object> data, IEnumerable<string> returnColumns)
        {
            return AsInsert(data, false, returnColumns);
        }

        /// <summary>
        /// Produces insert multi records
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="valuesCollection"></param>
        /// <param name="returnColumns"></param>
        /// <returns></returns>
        public Query AsInsert(IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection, IEnumerable<string> returnColumns = null)
        {
            var columnsList = columns?.ToList();
            var valuesCollectionList = valuesCollection?.ToList();
            var returnColumnsList = returnColumns?.ToList();

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
                    Values = valuesList,
                    ReturnColumns = returnColumnsList,
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