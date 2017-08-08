using System;

namespace SqlKata
{
    public partial class Query
    {

        public Query Select(params string[] columns)
        {
            foreach (var column in columns)
            {
                Add("select", new Column
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

            Add("select", new RawColumn
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });

            return this;
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
            Add("column", new QueryColumn
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