using System;
using System.Linq;
using SqlKata.Interfaces;

namespace SqlKata
{
    public partial class Query
    {

        public IQuery Select(params string[] columns)
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
        public IQuery SelectRaw(string expression, params object[] bindings)
        {
            Method = "select";

            AddComponent("select", new RawColumn
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });

            return this;
        }

        public IQuery Select(IQuery query, string alias)
        {
            Method = "select";

            query = query.Clone();

            AddComponent("select", new QueryColumn
            {
                Query = query.As(alias),
            });

            return this;
        }

        public IQuery Select(Func<IQuery, IQuery> callback, string alias)
        {
            return Select(callback.Invoke(NewChild()), alias);
        }
    }
}