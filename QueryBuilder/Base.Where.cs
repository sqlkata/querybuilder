using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public abstract partial class BaseQuery<Q>
    {
        public Q Where<T>(string column, string op, T value)
        {

            // If the value is "null", we will just assume the developer wants to add a
            // where null clause to the query. So, we will allow a short-cut here to
            // that method for convenience so the developer doesn't have to check.
            if (value == null)
            {
                return Not(op != "=").WhereNull(column);
            }

            return Add("where", new BasicCondition<T>
            {
                Column = column,
                Operator = op,
                Value = value,
                IsOr = getOr(),
                IsNot = getNot(),
            });
        }

        public Q Where<T>(string column, T value)
        {
            return Where(column, "=", value);
        }

        public Q Where<T>(Dictionary<string, T> values)
        {
            var query = (Q)this;
            var orFlag = getOr();
            var notFlag = getNot();

            foreach (var tuple in values)
            {
                if (orFlag)
                {
                    query = query.Or();
                }
                else
                {
                    query.And();
                }

                query = this.Not(notFlag).Where(tuple.Key, tuple.Value);
            }

            return query;
        }

        public Q Where<T>(IEnumerable<string> columns, IEnumerable<T> values)
        {
            if (columns.Count() == 0 || columns.Count() != values.Count())
            {
                throw new ArgumentException("Columns and Values count must match");
            }

            var query = (Q)this;

            var orFlag = getOr();
            var notFlag = getNot();

            for (var i = 0; i < columns.Count(); i++)
            {
                if (orFlag)
                {
                    query = query.Or();
                }
                else
                {
                    query.And();
                }

                query = this.Not(notFlag).Where(columns.ElementAt(i), values.ElementAt(i));
            }

            return query;
        }

        /// <summary>
        /// Apply the Where clause changes if the given "condition" is true.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Q WhereIf<T>(bool condition, string column, string op, T value)
        {
            if (condition)
            {
                return Where(column, op, value);
            }

            return (Q)this;
        }

        public Q WhereIf<T>(bool condition, string column, T value)
        {
            return WhereIf(condition, column, "=", value);
        }

        /// <summary>
        /// Apply the Or Where clause changes if the given "condition" is true.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Q OrWhereIf<T>(bool condition, string column, string op, T value)
        {
            if (condition)
            {
                return Or().Where(column, op, value);
            }

            return (Q)this;
        }

        public Q OrWhereIf<T>(bool condition, string column, T value)
        {
            return OrWhereIf(condition, column, "=", value);
        }

        public Q WhereRaw(string sql, params object[] bindings)
        {
            return Add("where", new RawCondition
            {
                Expression = sql,
                Bindings = Helper.Flatten(bindings).ToArray(),
                IsOr = getOr(),
                IsNot = getNot(),
            });
        }

        public Q OrWhereRaw(string sql, params object[] bindings)
        {
            return Or().WhereRaw(sql, bindings);
        }

        public Q Where(Func<Q, Q> callback)
        {
            var query = callback.Invoke(NewChild());

            /*
                if (Has("from") && Get("from").FirstOrDefault() is From)
                {
                    query.From((Get("from").FirstOrDefault() as From).Table);
                }
            */

            if (query.Has("from"))
            {
                return Add("where", new ExistsCondition<Q>
                {
                    Query = query,
                    IsNot = getNot(),
                    IsOr = getOr(),
                });
            }

            return Add("where", new NestedCondition<Q>
            {
                Query = query,
                IsNot = getNot(),
                IsOr = getOr(),
            });
        }

        public Q WhereNot(string column, string op, object value)
        {
            return Not(true).Where(column, op, value);
        }

        public Q WhereNot(string column, object value)
        {
            return WhereNot(column, "=", value);
        }

        public Q WhereNot(Func<Q, Q> callback)
        {
            return Not(true).Where(callback);
        }

        public Q OrWhere(string column, string op, object value)
        {
            return Or().Where(column, op, value);
        }
        public Q OrWhere(string column, object value)
        {
            return OrWhere(column, "=", value);
        }

        public Q OrWhere(Func<Q, Q> callback)
        {
            return Or().Where(callback);
        }
        public Q OrWhereNot(string column, string op, object value)
        {
            return this.Or().Not(true).Where(column, op, value);
        }
        public Q OrWhereNot(string column, object value)
        {
            return OrWhereNot(column, "=", value);
        }

        public Q OrWhereNot(Func<Q, Q> callback)
        {
            return Not(true).Or().Where(callback);
        }

        public Q WhereColumns(string first, string op, string second)
        {
            return Add("where", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = getOr(),
                IsNot = getNot(),
            });
        }

        public Q OrWhereColumns(string first, string op, string second)
        {
            return Or().WhereColumns(first, op, second);
        }

        public Q WhereNull(string column)
        {
            return Add("where", new NullCondition
            {
                Column = column,
                IsOr = getOr(),
                IsNot = getNot(),
            });
        }

        public Q WhereNotNull(string column)
        {
            return Not(true).WhereNull(column);
        }

        public Q OrWhereNull(string column)
        {
            return this.Or().WhereNull(column);
        }

        public Q OrWhereNotNull(string column)
        {
            return Or().Not(true).WhereNull(column);
        }

        public Q WhereLike(string column, string value, bool caseSensitive = false)
        {
            return Add("where", new BasicStringCondition
            {
                Operator = "like",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = getOr(),
                IsNot = getNot(),
            });
        }

        public Q OrWhereLike(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereLike(column, value, caseSensitive);
        }

        public Q OrWhereNotLike(string column, string value, bool caseSensitive = false)
        {
            return Or().Not(true).WhereLike(column, value, caseSensitive);
        }
        public Q WhereStarts(string column, string value, bool caseSensitive = false)
        {
            return Add("where", new BasicStringCondition
            {
                Operator = "starts",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = getOr(),
                IsNot = getNot(),
            });
        }

        public Q OrWhereStarts(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereStarts(column, value, caseSensitive);
        }

        public Q OrWhereNotStarts(string column, string value, bool caseSensitive = false)
        {
            return Or().Not(true).WhereStarts(column, value, caseSensitive);
        }

        public Q WhereEnds(string column, string value, bool caseSensitive = false)
        {
            return Add("where", new BasicStringCondition
            {
                Operator = "ends",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = getOr(),
                IsNot = getNot(),
            });
        }

        public Q OrWhereEnds(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereEnds(column, value, caseSensitive);
        }

        public Q OrWhereNotEnds(string column, string value, bool caseSensitive = false)
        {
            return Or().Not(true).WhereEnds(column, value, caseSensitive);
        }

        public Q WhereContains(string column, string value, bool caseSensitive = false)
        {
            return Add("where", new BasicStringCondition
            {
                Operator = "contains",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = getOr(),
                IsNot = getNot(),
            });
        }

        public Q OrWhereContains(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereContains(column, value, caseSensitive);
        }

        public Q OrWhereNotContains(string column, string value, bool caseSensitive = false)
        {
            return Or().Not(true).WhereContains(column, value, caseSensitive);
        }

        public Q WhereBetween<T>(string column, T lower, T higher)
        {
            return Add("where", new BetweenCondition<T>
            {
                Column = column,
                IsOr = getOr(),
                IsNot = getNot(),
                Lower = lower,
                Higher = higher
            });
        }

        public Q OrWhereBetween<T>(string column, T lower, T higher)
        {
            return Or().WhereBetween(column, lower, higher);
        }
        public Q WhereNotBetween<T>(string column, T lower, T higher)
        {
            return Not(true).WhereBetween(column, lower, higher);
        }
        public Q OrWhereNotBetween<T>(string column, T lower, T higher)
        {
            return Or().Not(true).WhereBetween(column, lower, higher);
        }

        public Q WhereIn<T>(string column, IEnumerable<T> values)
        {
            // If the developer has passed a string most probably he wants List<string>
            // since string is considered as List<char>
            if (values is string)
            {
                string val = values as string;

                return Add("where", new InCondition<string>
                {
                    Column = column,
                    IsOr = getOr(),
                    IsNot = getNot(),
                    Values = new List<string> { val }
                });
            }

            return Add("where", new InCondition<T>
            {
                Column = column,
                IsOr = getOr(),
                IsNot = getNot(),
                Values = values.Distinct().ToList()
            });


        }

        public Q OrWhereIn<T>(string column, IEnumerable<T> values)
        {
            return Or().WhereIn(column, values);
        }

        public Q WhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Not(true).WhereIn(column, values);
        }

        public Q OrWhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Or().Not(true).WhereIn(column, values);
        }


        public Q WhereIn(string column, Query query)
        {
            return Add("where", new InQueryCondition
            {
                Column = column,
                IsOr = getOr(),
                IsNot = getNot(),
                Query = query
            });
        }
        public Q WhereIn(string column, Func<Query, Query> callback)
        {
            var query = callback.Invoke(new Query());
            return WhereIn(column, query);
        }

        public Q OrWhereIn(string column, Query query)
        {
            return Or().WhereIn(column, query);
        }

        public Q OrWhereIn(string column, Func<Query, Query> callback)
        {
            return Or().WhereIn(column, callback);
        }
        public Q WhereNotIn(string column, Query query)
        {
            return Not(true).WhereIn(column, query);
        }

        public Q WhereNotIn(string column, Func<Query, Query> callback)
        {
            return Not(true).WhereIn(column, callback);
        }

        public Q OrWhereNotIn(string column, Query query)
        {
            return Or().Not(true).WhereIn(column, query);
        }

        public Q OrWhereNotIn(string column, Func<Query, Query> callback)
        {
            return Or().Not(true).WhereIn(column, callback);
        }


        /// <summary>
        /// Perform a sub query where clause
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Q Where(string column, string op, Func<Q, Q> callback)
        {
            var query = callback.Invoke(NewChild());

            return Where(column, op, query);
        }

        public Q Where(string column, string op, Query query)
        {
            return Add("where", new QueryCondition<Query>
            {
                Column = column,
                Operator = op,
                Query = query,
                IsNot = getNot(),
                IsOr = getOr(),
            });
        }

        public Q OrWhere(string column, string op, Query query)
        {
            return Or().Where(column, op, query);
        }
        public Q OrWhere(string column, string op, Func<Query, Query> callback)
        {
            return Or().Where(column, op, callback);
        }

        public Q WhereExists(Query query)
        {
            if (!query.Has("from"))
            {
                throw new ArgumentException(@"""FromClause"" cannot be empty if used inside of a ""WhereExists"" condition");
            }

            return Add("where", new ExistsCondition<Query>
            {
                Query = query,
                IsNot = getNot(),
                IsOr = getOr(),
            });
        }
        public Q WhereExists(Func<Query, Query> callback)
        {
            var childQuery = new Query().SetParent(this);
            return WhereExists(callback.Invoke(childQuery));
        }

        public Q WhereNotExists(Query query)
        {
            return Not(true).WhereExists(query);
        }

        public Q WhereNotExists(Func<Query, Query> callback)
        {
            return Not(true).WhereExists(callback);
        }

        public Q OrWhereExists(Query query)
        {
            return Or().WhereExists(query);
        }
        public Q OrWhereExists(Func<Query, Query> callback)
        {
            return Or().WhereExists(callback);
        }
        public Q OrWhereNotExists(Query query)
        {
            return Or().Not(true).WhereExists(query);
        }
        public Q OrWhereNotExists(Func<Query, Query> callback)
        {
            return Or().Not(true).WhereExists(callback);
        }

        public Q OrderBy(string column, bool ascending)
        {
            return Add("order", new OrderBy
            {
                Column = column,
                Ascending = ascending
            });

        }
    }
}