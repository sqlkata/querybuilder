using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers;

namespace SqlKata
{
    public partial class Query : BaseQuery<Query>
    {
        public bool _Distinct { get; set; } = false;
        public string _Alias { get; set; }

        protected List<string> operators = new List<string> {
            "=", "<", ">", "<=", ">=", "<>", "!=",
            "like", "like binary", "not like", "between", "ilike",
            "&", "|", "^", "<<", ">>",
            "rlike", "regexp", "not regexp",
            "~", "~*", "!~", "!~*", "similar to",
            "not similar to", "not ilike", "~~*", "!~~*",
        };

        public static Raw Raw(string value)
        {
            return new Raw(value);
        }

        public Query() : base()
        {
        }

        public override Query Clone()
        {
            var clone = base.Clone();
            clone._Alias = _Alias;
            clone._Distinct = _Distinct;
            return clone;
        }

        public Query As(string alias)
        {
            _Alias = alias;
            return this;
        }

        public Query Limit(int value)
        {
            var clause = GetOne("limit") as LimitOffset;

            if (clause != null)
            {
                clause.Limit = value;
                return this;
            }

            return Add("limit", new LimitOffset
            {
                Limit = value
            });
        }

        public Query Offset(int value)
        {
            var clause = GetOne("limit") as LimitOffset;

            if (clause != null)
            {
                clause.Offset = value;
                return this;
            }

            return Add("limit", new LimitOffset
            {
                Offset = value
            });
        }

        /// <summary>
        /// Alias for Limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Query Take(int limit)
        {
            return Limit(limit);
        }

        /// <summary>
        /// Alias for Offset
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Query Skip(int offset)
        {
            return Offset(offset);
        }

        /// <summary>
        /// Set the limit and offset for a given page.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public Query ForPage(int page, int perPage = 15)
        {
            return Skip((page - 1) * perPage).Take(perPage);
        }

        public Query First()
        {
            return this.Limit(1);
        }

        public Query Distinct()
        {
            _Distinct = true;
            return this;
        }

        /// <summary>
        /// Apply the callback's query changes if the given "condition" is true.
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
        /// Apply the callback's query changes if the given "condition" is false.
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



        public Query OrderBy(string column, string ordering = "asc")
        {
            return OrderBy(column, ordering.ToLower() == "asc");
        }

        public Query OrderByDesc(string column)
        {
            return OrderBy(column, false);
        }

        public Query OrderByRaw(string expression, params object[] bindings)
        {

            return Add("order", new RawOrderBy
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });

        }

        public Query OrderByRandom(string seed)
        {
            return Add("order", new OrderByRandom { });
        }

        public Query GroupBy(params string[] columns)
        {
            foreach (var column in columns)
            {
                Add("group", new Column
                {
                    Name = column
                });
            }

            return this;
        }

        public Query GroupByRaw(string expression, params object[] bindings)
        {
            Add("group", new RawColumn
            {
                Expression = expression
            });

            return this;
        }

        public Query GroupBy(Raw expression)
        {
            return GroupByRaw(expression.Value, expression.Bindings);
        }

        public override Query NewQuery()
        {
            return new Query();
        }

    }
}
