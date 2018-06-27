using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query : BaseQuery<Query>
    {
        #region Operators
        /// <summary>
        ///     A list of sql operators
        /// </summary>
        protected List<string> Operators = new List<string>
        {
            "=",
            "<",
            ">",
            "<=",
            ">=",
            "<>",
            "!=",
            "like",
            "like binary",
            "not like",
            "between",
            "ilike",
            "&",
            "|",
            "^",
            "<<",
            ">>",
            "rlike",
            "regexp",
            "not regexp",
            "~",
            "~*",
            "!~",
            "!~*",
            "similar to",
            "not similar to",
            "not ilike",
            "~~*",
            "!~~*"
        };
        #endregion

        #region BindingOrder
        protected override string[] BindingOrder
        {
            get
            {
                if (Method == "insert")
                {
                    return new[]
                    {
                        "cte", "insert"
                    };
                }

                if (Method == "update")
                {
                    return new[]
                    {
                        "cte", "update", "where"
                    };
                }

                if (Method == "delete")
                {
                    return new[]
                    {
                        "cte", "where"
                    };
                }

                return new[]
                {
                    "cte",
                    "select",
                    "from",
                    "join",
                    "where",
                    "group",
                    "having",
                    "order",
                    "limit",
                    "combine" // union, except, intersect
                };
            }
        }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override Query Clone()
        {
            var clone = base.Clone();
            clone.QueryAlias = QueryAlias;
            clone.IsDistinct = IsDistinct;
            clone.Method = Method;
            return clone;
        }
        #endregion

        #region As
        /// <summary>
        ///     Sets the alias for a <see cref="Query" />
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public Query As(string alias)
        {
            QueryAlias = alias;
            return this;
        }
        #endregion

        public Query For(string engine, Func<Query, Query> fn)
        {
            EngineScope = engine;

            var result = fn.Invoke(this);

            // reset the engine
            EngineScope = null;

            return result;
        }

        public Query With(Query query)
        {
            // Clear query alias and add it to the containing clause
            if (string.IsNullOrWhiteSpace(query.QueryAlias))
            {
                throw new InvalidOperationException("No Alias found for the CTE query");
            }

            var alias = query.QueryAlias.Trim();

            // clear the query alias
            query.QueryAlias = null;

            return AddComponent("cte", new QueryFromClause
            {
                Query = query.SetEngineScope(EngineScope),
                Alias = alias
            });
        }

        public Query With(Func<Query, Query> fn)
        {
            return With(fn.Invoke(new Query()));
        }

        public Query With(string alias, Query query)
        {
            return With(query.As(alias));
        }

        public Query With(string alias, Func<Query, Query> fn)
        {
            return With(alias, fn.Invoke(new Query()));
        }

        public Query WithRaw(string alias, string sql, params object[] bindings)
        {
            return AddComponent("cte", new RawFromClause
            {
                Alias = alias,
                Expression = sql,
                Bindings = Helper.Flatten(bindings).ToArray()
            });
        }

        public Query Limit(int value)
        {
            var clause = GetOneComponent("limit", EngineScope) as LimitOffset;

            if (clause != null)
            {
                clause.Limit = value;
                return this;
            }

            return AddComponent("limit", new LimitOffset
            {
                Limit = value
            });
        }

        public Query Offset(int value)
        {
            var clause = GetOneComponent("limit", EngineScope) as LimitOffset;

            if (clause != null)
            {
                clause.Offset = value;
                return this;
            }

            return AddComponent("limit", new LimitOffset
            {
                Offset = value
            });
        }

        /// <summary>
        ///     Alias for Limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Query Take(int limit)
        {
            return Limit(limit);
        }

        /// <summary>
        ///     Alias for Offset
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Query Skip(int offset)
        {
            return Offset(offset);
        }

        /// <summary>
        ///     Set the limit and offset for a set of rows
        /// </summary>
        /// <param name="startRow">The first row to select</param>
        /// <param name="rows">The amount fo rows</param>
        /// <returns></returns>
        public Query ForRows(int startRow, int rows = 15)
        {
            return Skip(startRow).Take(rows);
        }

        /// <summary>
        ///     Set the limit and offset for a given page.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public Query ForPage(int page, int perPage = 15)
        {
            return Skip((page - 1) * perPage).Take(perPage);
        }

        #region Distinct
        /// <summary>
        ///     Used to build a distinct <see cref="Select(string[])" /> query
        /// </summary>
        /// <remarks>
        ///     The SELECT DISTINCT statement is used to return only distinct (different) values.
        /// </remarks>
        /// <example>
        ///     SELECT DISTINCT VALUE1 FROM ....
        /// </example>
        /// <returns></returns>
        public Query Distinct()
        {
            IsDistinct = true;
            return this;
        }
        #endregion

        #region NewQuery
        /// <summary>
        ///     Returns a new empty <see cref="Query" />
        /// </summary>
        /// <returns></returns>
        public override Query NewQuery()
        {
            return new Query().SetEngineScope(EngineScope);
        }
        #endregion

        #region Properties
        public bool IsDistinct { get; set; }
        public string QueryAlias { get; set; }
        public string Method { get; set; } = "select";
        #endregion

        #region Constructors
        /// <summary>
        ///     Constructs a new query
        /// </summary>
        public Query()
        {
        }

        /// <summary>
        ///     Constructs a new query
        /// </summary>
        /// <param name="table">The table to select from</param>
        /// <param name="hints">Any hints to use on the table, e.g. nolock</param>
        public Query(string table, params string[] hints)
        {
            From(table, hints);
        }
        #endregion

        #region When
        /// <summary>
        ///     Apply the callback's query changes if the given "condition" is true.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Query When(bool condition, Func<Query, Query> callback)
        {
            if (condition)
            {
                return callback.Invoke(this);
            }

            return this;
        }

        /// <summary>
        ///     Apply the callback's query changes if the given "condition" is false.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Query WhenNot(bool condition, Func<Query, Query> callback)
        {
            if (!condition)
            {
                return callback.Invoke(this);
            }

            return this;
        }
        #endregion

        #region OrderBy
        /// <summary>
        ///     Sets the <paramref name="columns" /> that need to be used to order
        ///     the output of the <see cref="Query" />
        /// </summary>
        /// <remarks>
        ///     The ORDER BY keyword is used to sort the result-set in ascending order.
        /// </remarks>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Query OrderBy(params string[] columns)
        {
            foreach (var column in columns)
                AddComponent("order", new OrderBy
                {
                    Column = column,
                    Ascending = true
                });

            return this;
        }

        /// <summary>
        ///     Sets the <paramref name="columns" /> that need to be used to order
        ///     the output of the <see cref="Query" /> descending
        /// </summary>
        /// <remarks>
        ///     The ORDER BY keyword is used to sort the result-set in descending order.
        /// </remarks>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Query OrderByDesc(params string[] columns)
        {
            foreach (var column in columns)
                AddComponent("order", new OrderBy
                {
                    Column = column,
                    Ascending = false
                });

            return this;
        }

        /// <summary>
        ///     Set the RAW order by sql code
        /// </summary>
        /// <remarks>
        ///     Use this method when you cannot do what you want with the <see cref="OrderBy" />
        ///     and <see cref="OrderByDesc" /> methods
        /// </remarks>
        /// <param name="expression"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public Query OrderByRaw(string expression, params object[] bindings)
        {
            return AddComponent("order", new RawOrderBy
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });
        }

        /// <summary>
        ///     Order by a random <paramref name="seed" />
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public Query OrderByRandom(string seed)
        {
            return AddComponent("order", new OrderByRandom());
        }
        #endregion

        #region GroupBy
        /// <summary>
        ///     Group the output of the <see cref="Query" /> by the givin <paramref name="columns" />
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Query GroupBy(params string[] columns)
        {
            foreach (var column in columns)
                AddComponent("group", new Column
                {
                    Name = column
                });

            return this;
        }

        /// <summary>
        ///     Set the RAW group by sql code
        /// </summary>
        /// <remarks>
        ///     Use this method when you cannot do what you want with the <see cref="GroupBy" /> method
        /// </remarks>
        /// <param name="expression"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public Query GroupByRaw(string expression, params object[] bindings)
        {
            AddComponent("group", new RawColumn
            {
                Expression = expression,
                Bindings = bindings
            });

            return this;
        }
        #endregion
    }
}