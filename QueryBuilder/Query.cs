using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Interfaces;

namespace SqlKata
{
    public partial class Query: BaseQuery<IQuery>, IQuery
    {
        public bool IsDistinct { get; set; } = false;
        public string QueryAlias { get; set; }
        public string Method { get; set; } = "select";

        protected List<string> operators = new List<string> {
            "=", "<", ">", "<=", ">=", "<>", "!=", "<=>",
            "like", "like binary", "not like", "ilike",
            "&", "|", "^", "<<", ">>",
            "rlike", "regexp", "not regexp",
            "~", "~*", "!~", "!~*", "similar to",
            "not similar to", "not ilike", "~~*", "!~~*",
        };

        public Query() : base()
        {
        }

        public Query(string table) : base()
        {
            From(table);
        }

        public bool HasOffset(string engineCode = null)
        {
            var limitOffset = this.GetOneComponent<LimitOffset>("limit", engineCode);

            return limitOffset?.HasOffset() ?? false;
        }

        public bool HasLimit(string engineCode = null)
        {
            var limitOffset = this.GetOneComponent<LimitOffset>("limit", engineCode);

            return limitOffset?.HasLimit() ?? false;
        }

        public int GetOffset(string engineCode = null)
        {
            var limitOffset = this.GetOneComponent<LimitOffset>("limit", engineCode);

            return limitOffset?.Offset ?? 0;
        }

        public int GetLimit(string engineCode = null)
        {
            var limitOffset = this.GetOneComponent<LimitOffset>("limit", engineCode);

            return limitOffset?.Limit ?? 0;
        }

        public override IQuery Clone()
        {
            var clone = base.Clone();
            clone.QueryAlias = QueryAlias;
            clone.IsDistinct = IsDistinct;
            clone.Method = Method;
            return clone;
        }

        public IQuery As(string alias)
        {
            QueryAlias = alias;
            return this;
        }

        public IQuery For(string engine, Func<IQuery, IQuery> fn)
        {
            EngineScope = engine;

            var result = fn.Invoke(this);

            // reset the engine
            EngineScope = null;

            return result;
        }

        public IQuery With(IQuery query)
        {
            // Clear query alias and add it to the containing clause
            if (string.IsNullOrWhiteSpace(query.QueryAlias))
            {
                throw new InvalidOperationException("No Alias found for the CTE query");
            }

            query = query.Clone();

            var alias = query.QueryAlias.Trim();

            // clear the query alias
            query.QueryAlias = null;

            return AddComponent("cte", new QueryFromClause
            {
                Query = query,
                Alias = alias,
            });
        }

        public IQuery With(Func<IQuery, IQuery> fn)
        {
            return With(fn.Invoke(new Query()));
        }

        public IQuery With(string alias, IQuery query)
        {
            return With(query.As(alias));
        }

        public IQuery With(string alias, Func<IQuery, IQuery> fn)
        {
            return With(alias, fn.Invoke(new Query()));
        }

        public IQuery WithRaw(string alias, string sql, params object[] bindings)
        {
            return AddComponent("cte", new RawFromClause
            {
                Alias = alias,
                Expression = sql,
                Bindings = Helper.Flatten(bindings).ToArray(),
            });
        }

        public IQuery Limit(int value)
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

        public IQuery Offset(int value)
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
        /// Alias for Limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public IQuery Take(int limit)
        {
            return Limit(limit);
        }

        /// <summary>
        /// Alias for Offset
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public IQuery Skip(int offset)
        {
            return Offset(offset);
        }

        /// <summary>
        /// Set the limit and offset for a given page.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public IQuery ForPage(int page, int perPage = 15)
        {
            return Skip((page - 1) * perPage).Take(perPage);
        }

        public IQuery Distinct()
        {
            IsDistinct = true;
            return this;
        }

        /// <summary>
        /// Apply the callback's query changes if the given "condition" is true.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IQuery When(bool condition, Func<IQuery, IQuery> callback)
        {
            if (condition)
            {
                return callback.Invoke(this);
            }

            return this;
        }

        /// <summary>
        /// Apply the callback's query changes if the given "condition" is false.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IQuery WhenNot(bool condition, Func<IQuery, IQuery> callback)
        {
            if (!condition)
            {
                return callback.Invoke(this);
            }

            return this;
        }

        public IQuery OrderBy(params string[] columns)
        {
            foreach (var column in columns)
            {
                AddComponent("order", new OrderBy
                {
                    Column = column,
                    Ascending = true
                });
            }

            return this;
        }

        public IQuery OrderByDesc(params string[] columns)
        {
            foreach (var column in columns)
            {
                AddComponent("order", new OrderBy
                {
                    Column = column,
                    Ascending = false
                });
            }

            return this;
        }

        public IQuery OrderByRaw(string expression, params object[] bindings)
        {
            return AddComponent("order", new RawOrderBy
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });
        }

        public IQuery OrderByRandom(string seed)
        {
            return AddComponent("order", new OrderByRandom { });
        }

        public IQuery GroupBy(params string[] columns)
        {
            foreach (var column in columns)
            {
                AddComponent("group", new Column
                {
                    Name = column
                });
            }

            return this;
        }

        public IQuery GroupByRaw(string expression, params object[] bindings)
        {
            AddComponent("group", new RawColumn
            {
                Expression = expression,
                Bindings = bindings,
            });

            return this;
        }

        public override IQuery NewQuery()
        {
            return new Query();
        }

    }
}
