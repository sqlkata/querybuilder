using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query Returning(params string[] columns)
        {
            return Returning(columns.AsEnumerable());
        }

        public Query Returning(IEnumerable<string> columns)
        {
            var columnsList = columns?.ToList();

            if ((columnsList?.Count ?? 0) == 0)
            {
                throw new InvalidOperationException($"{nameof(columns)} cannot be null or empty");
            }

            columnsList = columnsList
                .Select(x => Helper.ExpandExpression(x))
                .SelectMany(x => x)
                .ToList();

            foreach (var column in columnsList)
            {
                AddComponent("returning", new Column
                {
                    Name = column
                });
            }

            return this;
        }

        public Query ReturningRaw(string sql, params object[] bindings)
        {
            return AddComponent("returning", new RawColumn
            {
                Expression = sql,
                Bindings = bindings,
            });
        }
    }
}
