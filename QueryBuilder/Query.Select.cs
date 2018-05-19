using System;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {

        public Query Select(params string[] columns)
        {
            Method = "select";

            foreach (var column in columns)
            {
                AddComponent("select", new Column
                {
                    Name = column
                });
            }

            return this;
        }

        /// <summary>
        /// Add a new "raw" select expression to the query.
        /// </summary>
        /// <returns></returns>
        public Query SelectRaw(string expression, params object[] bindings)
        {
            Method = "select";

            AddComponent("select", new RawColumn
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });

            return this;
        }

        public Query Select(Query query, string alias)
        {
            Method = "select";

            AddComponent("select", new QueryColumn
            {
                Query = query.As(alias).SetEngineScope(EngineScope),
            });

            return this;
        }

        public Query Select(Func<Query, Query> callback, string alias)
        {
            return Select(callback.Invoke(NewChild()), alias);
        }
    }
}