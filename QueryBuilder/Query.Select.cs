using System;
using System.Collections.Generic;
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

        public Query SelectCoalesce(Query query, string alias, params string[] fallbackColumns)
        {
            var fallbacks = fallbackColumns.Select(t => new CoalesceFallback(t, FallbackType.Column)).ToList();
            return SelectCoalesce(query, alias, fallbacks);
        }

        public Query SelectCoalesce(Func<Query, Query> callback, string alias, params string[] fallbackColumns)
        {
            return SelectCoalesce(callback.Invoke(NewQuery()), alias, fallbackColumns);
        }

        public Query SelectCoalesce(Query query, string alias, List<CoalesceFallback> fallbacks)
        {
            Method = "select";

            AddComponent("select", new CoalesceColumn
            {
                Query = query.As(alias),
                Fallbacks = fallbacks
            });

            return this;
        }

        public Query SelectCoalesce(Func<Query, Query> callback, string alias, List<CoalesceFallback> fallbacks)
        {
            return SelectCoalesce(callback.Invoke(NewQuery()), alias, fallbacks);
        }

        public Query Select(params object[] columns)
        {
            foreach (var item in columns)
            {
                if (item is Raw)
                {
                    SelectRaw((item as Raw).Value, (item as Raw).Bindings);
                }
                else if (item is string)
                {
                    Select((string)item);
                }
                else
                {
                    throw new ArgumentException("only string and Raw are allowed");
                }
            }

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