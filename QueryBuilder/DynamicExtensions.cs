using System.Collections.Concurrent;
using System.Reflection;

namespace SqlKata
{
    public sealed class DynamicData
    {
        public required Dictionary<string, object?> Properties { get; init; }
        public required Dictionary<string, object?> Keys { get; init; }
    }
    public static class DynamicExtensions
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> CacheDictionaryProperties = new();

        /// <summary>
        ///     Gather a list of key-values representing the properties of the object and their values.
        /// </summary>
        /// <param name="data">The plain C# object</param>
        /// <returns></returns>
        public static DynamicData ToDynamicData(this object data)
        {
            var properties = new Dictionary<string, object?>();
            var keys = new Dictionary<string, object?>();
            var props = CacheDictionaryProperties.GetOrAdd(data.GetType(),
                type => type.GetRuntimeProperties().ToArray());

            foreach (var property in props)
            {
                if (property.GetCustomAttribute(typeof(IgnoreAttribute)) != null) continue;

                var value = property.GetValue(data);

                var colAttr = property.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;

                var name = colAttr?.Name ?? property.Name;

                properties.Add(name, value);

                if (colAttr is KeyAttribute)
                    keys.Add(name, value);
            }

            return new DynamicData { Properties = properties, Keys = keys };
        }

    }
}
