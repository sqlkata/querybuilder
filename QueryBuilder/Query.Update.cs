namespace SqlKata
{
    public partial class Query
    {
        public Query AsUpdate(object data)
        {
            var dictionary = BuildKeyValuePairsFromObject(data, true);

            return AsUpdate(dictionary);
        }

        public Query AsUpdate(IEnumerable<string> columns, IEnumerable<object> values)
        {
            var columnsCache = columns is ICollection<string> c ? c : columns.ToArray();
            var valuesCache = values is ICollection<object> v ? v : values.ToArray();
            if (!columnsCache.Any() || !valuesCache.Any())
                throw new InvalidOperationException($"{columnsCache} and {valuesCache} cannot be null or empty");

            if (columnsCache.Count != valuesCache.Count)
                throw new InvalidOperationException($"{columnsCache} count should be equal to {valuesCache} count");

            Method = "update";

            ClearComponent("update").AddComponent("update", new InsertClause
            {
                Columns = columnsCache.ToList(),
                Values = valuesCache.ToList()
            });

            return this;
        }

        public Query AsUpdate(IEnumerable<KeyValuePair<string, object>> values)
        {
            var valuesCached = values is IReadOnlyDictionary<string, object> d
                ? d
                : values.ToDictionary(x => x.Key, x => x.Value);
            if (valuesCached == null || valuesCached.Any() == false)
                throw new InvalidOperationException($"{valuesCached} cannot be null or empty");

            Method = "update";

            ClearComponent("update").AddComponent("update", new InsertClause
            {
                Columns = valuesCached.Select(x => x.Key).ToList(),
                Values = valuesCached.Select(x => x.Value).ToList()
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
