using System.Collections.Immutable;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsInsert(object data, bool returnId = false)
        {
            var propertiesKeyValues = BuildKeyValuePairsFromObject(data);

            return AsInsert(propertiesKeyValues, returnId);
        }

        public Query AsInsert(IEnumerable<string> columns, IEnumerable<object?> values)
        {
            ArgumentNullException.ThrowIfNull(columns);
            ArgumentNullException.ThrowIfNull(values);

            var columnsList = columns.ToList();
            var valuesList = values.ToImmutableArray();

            if (columnsList.Count == 0 || valuesList.Length == 0)
                throw new InvalidOperationException($"{nameof(columns)} and {nameof(values)} cannot be null or empty");

            if (columnsList.Count != valuesList.Length)
                throw new InvalidOperationException($"{nameof(columns)} and {nameof(values)} cannot be null or empty");

            Method = "insert";

            RemoveComponent("insert").AddComponent(new InsertClause
            {
                Engine = EngineScope,
                Component = "insert",
                Columns = columnsList,
                Values = valuesList
            });

            return this;
        }

        public Query AsInsert(IEnumerable<KeyValuePair<string, object?>> values, bool returnId = false)
        {
            var valuesCached = values is IReadOnlyDictionary<string, object?> d
                ? d
                : values.ToDictionary(x => x.Key, x => x.Value);
            if (valuesCached == null || valuesCached.Count == 0)
                throw new InvalidOperationException($"{valuesCached} argument cannot be null or empty");

            Method = "insert";

            RemoveComponent("insert").AddComponent(new InsertClause
            {
                Engine = EngineScope,
                Component = "insert",

                Columns = valuesCached.Select(x => x.Key).ToList(),
                Values = valuesCached.Select(x => x.Value).ToImmutableArray(),
                ReturnId = returnId
            });

            return this;
        }

        /// <summary>
        ///     Produces insert multi records
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="rowsValues"></param>
        /// <returns></returns>
        public Query AsInsert(IEnumerable<string> columns, IEnumerable<IEnumerable<object>> rowsValues)
        {
            var columnsList = columns?.ToList();
            var valuesCollectionList = rowsValues?.ToList();

            if ((columnsList?.Count ?? 0) == 0 || (valuesCollectionList?.Count ?? 0) == 0)
                throw new InvalidOperationException(
                    $"{nameof(columns)} and {nameof(rowsValues)} cannot be null or empty");

            Method = "insert";

            RemoveComponent("insert");

            foreach (var values in valuesCollectionList)
            {
                var valuesList = values.ToImmutableArray();
                if (columnsList.Count != valuesList.Length)
                    throw new InvalidOperationException(
                        $"{nameof(columns)} count should be equal to each {nameof(rowsValues)} entry count");

                AddComponent(new InsertClause
                {
                    Engine = EngineScope,
                    Component = "insert",

                    Columns = columnsList,
                    Values = valuesList
                });
            }

            return this;
        }

        /// <summary>
        ///     Produces insert from subQuery
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public Query AsInsert(IEnumerable<string> columns, Query query)
        {
            Method = "insert";

            RemoveComponent("insert").AddComponent(new InsertQueryClause
            {
                Engine = EngineScope,
                Component = "insert",
                Columns = columns.ToList(),
                Query = query.Clone()
            });

            return this;
        }
    }
}
