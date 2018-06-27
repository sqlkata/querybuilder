using System;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        /// <summary>
        ///     Used to build the select part of a <see cref="Query" />
        /// </summary>
        /// <remarks>
        ///     The SELECT statement is used to select data from a database.
        /// </remarks>
        /// <param name="columns">The columns to select</param>
        /// <returns></returns>
        public Query Select(params string[] columns)
        {
            Method = "select";

            foreach (var column in columns)
                AddComponent("select", new Column
                {
                    Name = column
                });

            return this;
        }

        /// <summary>
        ///     Add a new "raw" select expression to the <see cref="Query" />
        /// </summary>
        /// <remarks>
        ///     The SELECT statement is used to select data from a database.
        /// </remarks>
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

        /// <summary>
        ///     Used to build a sub select in the select part of a <see cref="Query" />
        /// </summary>
        /// <remarks>
        ///     The SELECT statement is used to select data from a database.
        /// </remarks>
        /// <param name="query"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public Query Select(Query query, string alias)
        {
            Method = "select";

            AddComponent("select", new QueryColumn
            {
                Query = query.As(alias).SetEngineScope(EngineScope)
            });

            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public Query Select(Func<Query, Query> callback, string alias)
        {
            return Select(callback.Invoke(NewChild()), alias);
        }
    }
}