using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata
{
    public abstract partial class BaseQuery<Q>
    {
        public Q Where(string column, string op, object value)
        {

            // If the value is "null", we will just assume the developer wants to add a
            // where null clause to the query. So, we will allow a short-cut here to
            // that method for convenience so the developer doesn't have to check.
            if (value == null)
            {
                return Not(op != "=").WhereNull(column);
            }

            return AddComponent("where", new BasicCondition
            {
                Column = column,
                Operator = op,
                Value = value,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNot(string column, string op, object value)
        {
            return Not().Where(column, op, value);
        }

        public Q OrWhere(string column, string op, object value)
        {
            return Or().Where(column, op, value);
        }

        public Q OrWhereNot(string column, string op, object value)
        {
            return this.Or().Not().Where(column, op, value);
        }

        public Q Where(string column, object value)
        {
            return Where(column, "=", value);
        }
        public Q WhereNot(string column, object value)
        {
            return WhereNot(column, "=", value);
        }
        public Q OrWhere(string column, object value)
        {
            return OrWhere(column, "=", value);
        }
        public Q OrWhereNot(string column, object value)
        {
            return OrWhereNot(column, "=", value);
        }

        /// <summary>
        /// Perform a where constraint
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public Q Where(object constraints)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var item in constraints.GetType().GetRuntimeProperties())
            {
                dictionary.Add(item.Name, item.GetValue(constraints));
            }

            return Where(dictionary);
        }

        public Q Where(IReadOnlyDictionary<string, object> values)
        {
            var query = (Q)this;
            var orFlag = GetOr();
            var notFlag = GetNot();

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

        public Q WhereRaw(string sql, params object[] bindings)
        {
            return AddComponent("where", new RawCondition
            {
                Expression = sql,
                Bindings = bindings,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q OrWhereRaw(string sql, params object[] bindings)
        {
            return Or().WhereRaw(sql, bindings);
        }

        /// <summary>
        /// Apply a nested where clause
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Q Where(Func<Q, Q> callback)
        {
            var query = callback.Invoke(NewChild());

            // omit empty queries
            if (!query.Clauses.Where(x => x.Component == "where").Any())
            {
                return (Q)this;
            }

            return AddComponent("where", new NestedCondition<Q>
            {
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr(),
            });
        }

        public Q WhereNot(Func<Q, Q> callback)
        {
            return Not().Where(callback);
        }

        public Q OrWhere(Func<Q, Q> callback)
        {
            return Or().Where(callback);
        }

        public Q OrWhereNot(Func<Q, Q> callback)
        {
            return Not().Or().Where(callback);
        }

        public Q WhereColumns(string first, string op, string second)
        {
            return AddComponent("where", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q OrWhereColumns(string first, string op, string second)
        {
            return Or().WhereColumns(first, op, second);
        }

        public Q WhereNull(string column)
        {
            return AddComponent("where", new NullCondition
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNotNull(string column)
        {
            return Not().WhereNull(column);
        }

        public Q OrWhereNull(string column)
        {
            return this.Or().WhereNull(column);
        }

        public Q OrWhereNotNull(string column)
        {
            return Or().Not().WhereNull(column);
        }

        public Q WhereTrue(string column)
        {
            return AddComponent("where", new BooleanCondition
            {
                Column = column,
                Value = true,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q OrWhereTrue(string column)
        {
            return Or().WhereTrue(column);
        }

        public Q WhereFalse(string column)
        {
            return AddComponent("where", new BooleanCondition
            {
                Column = column,
                Value = false,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q OrWhereFalse(string column)
        {
            return Or().WhereFalse(column);
        }

        public Q WhereLike(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "like",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNotLike(string column, string value, bool caseSensitive = false)
        {
            return Not().WhereLike(column, value, caseSensitive);
        }

        public Q OrWhereLike(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereLike(column, value, caseSensitive);
        }

        public Q OrWhereNotLike(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().WhereLike(column, value, caseSensitive);
        }
        public Q WhereStarts(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "starts",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNotStarts(string column, string value, bool caseSensitive = false)
        {
            return Not().WhereStarts(column, value, caseSensitive);
        }

        public Q OrWhereStarts(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereStarts(column, value, caseSensitive);
        }

        public Q OrWhereNotStarts(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().WhereStarts(column, value, caseSensitive);
        }

        public Q WhereEnds(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "ends",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNotEnds(string column, string value, bool caseSensitive = false)
        {
            return Not().WhereEnds(column, value, caseSensitive);
        }

        public Q OrWhereEnds(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereEnds(column, value, caseSensitive);
        }

        public Q OrWhereNotEnds(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().WhereEnds(column, value, caseSensitive);
        }

        public Q WhereContains(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "contains",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNotContains(string column, string value, bool caseSensitive = false)
        {
            return Not().WhereContains(column, value, caseSensitive);
        }

        public Q OrWhereContains(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereContains(column, value, caseSensitive);
        }

        public Q OrWhereNotContains(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().WhereContains(column, value, caseSensitive);
        }

        public Q WhereBetween<T>(string column, T lower, T higher)
        {
            return AddComponent("where", new BetweenCondition<T>
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
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
            return Not().WhereBetween(column, lower, higher);
        }
        public Q OrWhereNotBetween<T>(string column, T lower, T higher)
        {
            return Or().Not().WhereBetween(column, lower, higher);
        }

        public Q WhereIn<T>(string column, IEnumerable<T> values)
        {
            // If the developer has passed a string most probably he wants List<string>
            // since string is considered as List<char>
            if (values is string)
            {
                string val = values as string;

                return AddComponent("where", new InCondition<string>
                {
                    Column = column,
                    IsOr = GetOr(),
                    IsNot = GetNot(),
                    Values = new List<string> { val }
                });
            }

            return AddComponent("where", new InCondition<T>
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Values = values.Distinct().ToList()
            });


        }

        public Q OrWhereIn<T>(string column, IEnumerable<T> values)
        {
            return Or().WhereIn(column, values);
        }

        public Q WhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Not().WhereIn(column, values);
        }

        public Q OrWhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Or().Not().WhereIn(column, values);
        }


        public Q WhereIn(string column, Query query)
        {
            return AddComponent("where", new InQueryCondition
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Query = query,
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
            return Not().WhereIn(column, query);
        }

        public Q WhereNotIn(string column, Func<Query, Query> callback)
        {
            return Not().WhereIn(column, callback);
        }

        public Q OrWhereNotIn(string column, Query query)
        {
            return Or().Not().WhereIn(column, query);
        }

        public Q OrWhereNotIn(string column, Func<Query, Query> callback)
        {
            return Or().Not().WhereIn(column, callback);
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
            return AddComponent("where", new QueryCondition<Query>
            {
                Column = column,
                Operator = op,
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr(),
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
            if (!query.HasComponent("from"))
            {
                throw new ArgumentException("'FromClause' cannot be empty if used inside a 'WhereExists' condition");
            }

            // remove unneeded components
            query = query.Clone().ClearComponent("select")
                .SelectRaw("1")
                .Limit(1);

            return AddComponent("where", new ExistsCondition
            {
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr(),
            });
        }
        public Q WhereExists(Func<Query, Query> callback)
        {
            var childQuery = new Query().SetParent(this);
            return WhereExists(callback.Invoke(childQuery));
        }

        public Q WhereNotExists(Query query)
        {
            return Not().WhereExists(query);
        }

        public Q WhereNotExists(Func<Query, Query> callback)
        {
            return Not().WhereExists(callback);
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
            return Or().Not().WhereExists(query);
        }
        public Q OrWhereNotExists(Func<Query, Query> callback)
        {
            return Or().Not().WhereExists(callback);
        }

        #region date
        public Q WhereDatePart(string part, string column, string op, object value)
        {
            return AddComponent("where", new BasicDateCondition
            {
                Operator = op,
                Column = column,
                Value = value,
                Part = part,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }
        public Q WhereNotDatePart(string part, string column, string op, object value)
        {
            return Not().WhereDatePart(part, column, op, value);
        }

        public Q OrWhereDatePart(string part, string column, string op, object value)
        {
            return Or().WhereDatePart(part, column, op, value);
        }

        public Q OrWhereNotDatePart(string part, string column, string op, object value)
        {
            return Or().Not().WhereDatePart(part, column, op, value);
        }

        public Q WhereDate(string column, string op, object value)
        {
            return WhereDatePart("date", column, op, value);
        }
        public Q WhereNotDate(string column, string op, object value)
        {
            return Not().WhereDate(column, op, value);
        }
        public Q OrWhereDate(string column, string op, object value)
        {
            return Or().WhereDate(column, op, value);
        }
        public Q OrWhereNotDate(string column, string op, object value)
        {
            return Or().Not().WhereDate(column, op, value);
        }

        public Q WhereTime(string column, string op, object value)
        {
            return WhereDatePart("time", column, op, value);
        }
        public Q WhereNotTime(string column, string op, object value)
        {
            return Not().WhereTime(column, op, value);
        }
        public Q OrWhereTime(string column, string op, object value)
        {
            return Or().WhereTime(column, op, value);
        }
        public Q OrWhereNotTime(string column, string op, object value)
        {
            return Or().Not().WhereTime(column, op, value);
        }

        public Q WhereDatePart(string part, string column, object value)
        {
            return WhereDatePart(part, column, "=", value);
        }
        public Q WhereNotDatePart(string part, string column, object value)
        {
            return WhereNotDatePart(part, column, "=", value);
        }

        public Q OrWhereDatePart(string part, string column, object value)
        {
            return OrWhereDatePart(part, column, "=", value);
        }

        public Q OrWhereNotDatePart(string part, string column, object value)
        {
            return OrWhereNotDatePart(part, column, "=", value);
        }

        public Q WhereDate(string column, object value)
        {
            return WhereDate(column, "=", value);
        }
        public Q WhereNotDate(string column, object value)
        {
            return WhereNotDate(column, "=", value);
        }
        public Q OrWhereDate(string column, object value)
        {
            return OrWhereDate(column, "=", value);
        }
        public Q OrWhereNotDate(string column, object value)
        {
            return OrWhereNotDate(column, "=", value);
        }

        public Q WhereTime(string column, object value)
        {
            return WhereTime(column, "=", value);
        }
        public Q WhereNotTime(string column, object value)
        {
            return WhereNotTime(column, "=", value);
        }
        public Q OrWhereTime(string column, object value)
        {
            return OrWhereTime(column, "=", value);
        }
        public Q OrWhereNotTime(string column, object value)
        {
            return OrWhereNotTime(column, "=", value);
        }

        #endregion

    }
}