using System.Collections.Generic;
using System.Linq;

namespace SqlKata.Extensions
{
    public static class CollectionExtensions
    {
        public static Dictionary<string, object> MergeKeysAndValues(this List<string> keys, List<object> values)
        {
            var data = new Dictionary<string, object>();

            for (var i = 0; i < keys.Count; i++)
            {
                data.Add(keys[i], values[i]);
            }

            return data;
        }

        public static Dictionary<string, object> CreateDictionary(this IEnumerable<KeyValuePair<string, object>> values)
        {
            if (values is Dictionary<string, object> dictionary)
            {
                return dictionary;
            }

            return values.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
