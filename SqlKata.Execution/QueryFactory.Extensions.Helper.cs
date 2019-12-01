using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlKata.Execution
{
    public static class QueryFactoryHelper
    {
        public static List<Dictionary<string, object>> GetDynamicResult<T>(IEnumerable<T> result)
        {
            return result
                .Cast<IDictionary<string, object>>()
                .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }
        public static List<string> GetForeignIDs(List<Dictionary<string, object>> dynamicResult, Include include)
        {
            return dynamicResult.Where(x => x[include.ForeignKey] != null)
                .Select(x => x[include.ForeignKey].ToString())
                .ToList();
        }
        public static void TryFetchingForeignKey(Query query, Include include)
        {
            if (include.ForeignKey == null)
            {
                // try to guess the default key
                // I will try to fetch the table name if provided and appending the Id as a convention
                // Here am using Humanizer package to help getting the singular form of the table

                var fromTable = query.GetOneComponent("from") as FromClause;

                if (fromTable == null)
                {
                    throw new InvalidOperationException($"Cannot guess the foreign key for the included relation '{include.Name}'");
                }

                var table = fromTable.Alias ?? fromTable.Table;

                include.ForeignKey = table.Singularize(false) + "Id";
            }
        }
        public static List<string> GetLocalIDs(List<Dictionary<string, object>> dynamicResult, Include include)
        {
            return dynamicResult.Where(x => x[include.LocalKey] != null)
            .Select(x => x[include.LocalKey].ToString())
            .ToList();
        }
    }
}
