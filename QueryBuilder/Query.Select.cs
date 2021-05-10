using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public Query Select(params string[] columns)
        {
            return SelectAs(
                columns
                .Select(x => (x, null as string))
                .ToArray()
            );
        }

        /// <summary>
        /// Select columns with an alias
        /// </summary>
        /// <returns></returns>
        public Query SelectAs(params (string, string)[] columns)
        {
            Method = "select";

            columns = columns
                .Select(x => Helper.ExpandExpression(x.Item1).Select(y => (y, x.Item2)))
                .SelectMany(x => x)
                .ToArray();

            foreach (var column in columns)
            {
                AddComponent("select", new Column
                {
                    Name = column.Item1,
                    Alias = column.Item2
                });
            }

            return this;
        }

        /// <summary>
        /// Add a new "raw" select expression to the query.
        /// </summary>
        /// <returns></returns>
        public Query SelectRaw(string sql, params object[] bindings)
        {
            Method = "select";

            AddComponent("select", new RawColumn
            {
                Expression = sql,
                Bindings = bindings,
            });

            return this;
        }

        public Query Select(Query query, string alias)
        {
            Method = "select";

            query = query.Clone();

            AddComponent("select", new QueryColumn
            {
                Query = query.As(alias),
            });

            return this;
        }

        public Query Select(Func<Query, Query> callback, string alias)
        {
            return Select(callback.Invoke(NewChild()), alias);
        }
    }
}
