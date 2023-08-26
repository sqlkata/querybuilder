using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata
{
    public abstract partial class BaseQuery<TQ>
    {
        public TQ Where(string column, string op, object value)
        {
            // If the value is "null", we will just assume the developer wants to add a
            // where null clause to the query. So, we will allow a short-cut here to
            // that method for convenience so the developer doesn't have to check.
            if (value == null) return Not(op != "=").WhereNull(column);

            if (value is bool boolValue)
            {
                if (op != "=") Not();

                return boolValue ? WhereTrue(column) : WhereFalse(column);
            }

            return AddComponent("where", new BasicCondition
            {
                Column = column,
                Operator = op,
                Value = value,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public TQ WhereNot(string column, string op, object value)
        {
            return Not().Where(column, op, value);
        }

        public TQ OrWhere(string column, string op, object value)
        {
            return Or().Where(column, op, value);
        }

        public TQ OrWhereNot(string column, string op, object value)
        {
            return Or().Not().Where(column, op, value);
        }

        public TQ Where(string column, object value)
        {
            return Where(column, "=", value);
        }

        public TQ WhereNot(string column, object value)
        {
            return WhereNot(column, "=", value);
        }

        public TQ OrWhere(string column, object value)
        {
            return OrWhere(column, "=", value);
        }

        public TQ OrWhereNot(string column, object value)
        {
            return OrWhereNot(column, "=", value);
        }

        /// <summary>
        ///     Perform a where constraint
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public TQ Where(object constraints)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var item in constraints.GetType().GetRuntimeProperties())
                dictionary.Add(item.Name, item.GetValue(constraints));

            return Where(dictionary);
        }

        public TQ Where(IEnumerable<KeyValuePair<string, object>> values)
        {
            var query = (TQ)this;
            var orFlag = GetOr();
            var notFlag = GetNot();

            foreach (var tuple in values)
            {
                if (orFlag)
                    query = query.Or();
                else
                    query.And();

                query = Not(notFlag).Where(tuple.Key, tuple.Value);
            }

            return query;
        }

        public TQ WhereRaw(string sql, params object[] bindings)
        {
            return AddComponent("where", new RawCondition
            {
                Expression = sql,
                Bindings = bindings,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public TQ OrWhereRaw(string sql, params object[] bindings)
        {
            return Or().WhereRaw(sql, bindings);
        }

        /// <summary>
        ///     Apply a nested where clause
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public TQ Where(Func<TQ, TQ> callback)
        {
            var query = callback.Invoke(NewChild());

            // omit empty queries
            if (!query.Clauses.Where(x => x.Component == "where").Any()) return (TQ)this;

            return AddComponent("where", new NestedCondition<TQ>
            {
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public TQ WhereNot(Func<TQ, TQ> callback)
        {
            return Not().Where(callback);
        }

        public TQ OrWhere(Func<TQ, TQ> callback)
        {
            return Or().Where(callback);
        }

        public TQ OrWhereNot(Func<TQ, TQ> callback)
        {
            return Not().Or().Where(callback);
        }

        public TQ WhereColumns(string first, string op, string second)
        {
            return AddComponent("where", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public TQ OrWhereColumns(string first, string op, string second)
        {
            return Or().WhereColumns(first, op, second);
        }

        public TQ WhereNull(string column)
        {
            return AddComponent("where", new NullCondition
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public TQ WhereNotNull(string column)
        {
            return Not().WhereNull(column);
        }

        public TQ OrWhereNull(string column)
        {
            return Or().WhereNull(column);
        }

        public TQ OrWhereNotNull(string column)
        {
            return Or().Not().WhereNull(column);
        }

        public TQ WhereTrue(string column)
        {
            return AddComponent("where", new BooleanCondition
            {
                Column = column,
                Value = true,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public TQ OrWhereTrue(string column)
        {
            return Or().WhereTrue(column);
        }

        public TQ WhereFalse(string column)
        {
            return AddComponent("where", new BooleanCondition
            {
                Column = column,
                Value = false,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public TQ OrWhereFalse(string column)
        {
            return Or().WhereFalse(column);
        }

        public TQ WhereLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "like",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public TQ WhereNotLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return Not().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public TQ OrWhereLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return Or().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public TQ OrWhereNotLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return Or().Not().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public TQ WhereStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "starts",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public TQ WhereNotStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return Not().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public TQ OrWhereStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return Or().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public TQ OrWhereNotStarts(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Or().Not().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public TQ WhereEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "ends",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public TQ WhereNotEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return Not().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public TQ OrWhereEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return Or().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public TQ OrWhereNotEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return Or().Not().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public TQ WhereContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "contains",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public TQ WhereNotContains(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Not().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public TQ OrWhereContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return Or().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public TQ OrWhereNotContains(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Or().Not().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public TQ WhereBetween<T>(string column, T lower, T higher)
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

        public TQ OrWhereBetween<T>(string column, T lower, T higher)
        {
            return Or().WhereBetween(column, lower, higher);
        }

        public TQ WhereNotBetween<T>(string column, T lower, T higher)
        {
            return Not().WhereBetween(column, lower, higher);
        }

        public TQ OrWhereNotBetween<T>(string column, T lower, T higher)
        {
            return Or().Not().WhereBetween(column, lower, higher);
        }

        public TQ WhereIn<T>(string column, IEnumerable<T> values)
        {
            // If the developer has passed a string they most likely want a List<string>
            // since string is considered as List<char>
            if (values is string)
            {
                var val = values as string;

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

        public TQ OrWhereIn<T>(string column, IEnumerable<T> values)
        {
            return Or().WhereIn(column, values);
        }

        public TQ WhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Not().WhereIn(column, values);
        }

        public TQ OrWhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Or().Not().WhereIn(column, values);
        }


        public TQ WhereIn(string column, Query query)
        {
            return AddComponent("where", new InQueryCondition
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Query = query
            });
        }

        public TQ WhereIn(string column, Func<Query, Query> callback)
        {
            var query = callback.Invoke(new Query().SetParent(this));

            return WhereIn(column, query);
        }

        public TQ OrWhereIn(string column, Query query)
        {
            return Or().WhereIn(column, query);
        }

        public TQ OrWhereIn(string column, Func<Query, Query> callback)
        {
            return Or().WhereIn(column, callback);
        }

        public TQ WhereNotIn(string column, Query query)
        {
            return Not().WhereIn(column, query);
        }

        public TQ WhereNotIn(string column, Func<Query, Query> callback)
        {
            return Not().WhereIn(column, callback);
        }

        public TQ OrWhereNotIn(string column, Query query)
        {
            return Or().Not().WhereIn(column, query);
        }

        public TQ OrWhereNotIn(string column, Func<Query, Query> callback)
        {
            return Or().Not().WhereIn(column, callback);
        }


        /// <summary>
        ///     Perform a sub query where clause
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public TQ Where(string column, string op, Func<TQ, TQ> callback)
        {
            var query = callback.Invoke(NewChild());

            return Where(column, op, query);
        }

        public TQ Where(string column, string op, Query query)
        {
            return AddComponent("where", new QueryCondition<Query>
            {
                Column = column,
                Operator = op,
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public TQ WhereSub(Query query, object value)
        {
            return WhereSub(query, "=", value);
        }

        public TQ WhereSub(Query query, string op, object value)
        {
            return AddComponent("where", new SubQueryCondition<Query>
            {
                Value = value,
                Operator = op,
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public TQ OrWhereSub(Query query, object value)
        {
            return Or().WhereSub(query, value);
        }

        public TQ OrWhereSub(Query query, string op, object value)
        {
            return Or().WhereSub(query, op, value);
        }

        public TQ OrWhere(string column, string op, Query query)
        {
            return Or().Where(column, op, query);
        }

        public TQ OrWhere(string column, string op, Func<Query, Query> callback)
        {
            return Or().Where(column, op, callback);
        }

        public TQ WhereExists(Query query)
        {
            if (!query.HasComponent("from"))
                throw new ArgumentException(
                    $"'{nameof(FromClause)}' cannot be empty if used inside a '{nameof(WhereExists)}' condition");

            return AddComponent("where", new ExistsCondition
            {
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public TQ WhereExists(Func<Query, Query> callback)
        {
            var childQuery = new Query().SetParent(this);
            return WhereExists(callback.Invoke(childQuery));
        }

        public TQ WhereNotExists(Query query)
        {
            return Not().WhereExists(query);
        }

        public TQ WhereNotExists(Func<Query, Query> callback)
        {
            return Not().WhereExists(callback);
        }

        public TQ OrWhereExists(Query query)
        {
            return Or().WhereExists(query);
        }

        public TQ OrWhereExists(Func<Query, Query> callback)
        {
            return Or().WhereExists(callback);
        }

        public TQ OrWhereNotExists(Query query)
        {
            return Or().Not().WhereExists(query);
        }

        public TQ OrWhereNotExists(Func<Query, Query> callback)
        {
            return Or().Not().WhereExists(callback);
        }

        #region date

        public TQ WhereDatePart(string part, string column, string op, object value)
        {
            return AddComponent("where", new BasicDateCondition
            {
                Operator = op,
                Column = column,
                Value = value,
                Part = part?.ToLowerInvariant(),
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public TQ WhereNotDatePart(string part, string column, string op, object value)
        {
            return Not().WhereDatePart(part, column, op, value);
        }

        public TQ OrWhereDatePart(string part, string column, string op, object value)
        {
            return Or().WhereDatePart(part, column, op, value);
        }

        public TQ OrWhereNotDatePart(string part, string column, string op, object value)
        {
            return Or().Not().WhereDatePart(part, column, op, value);
        }

        public TQ WhereDate(string column, string op, object value)
        {
            return WhereDatePart("date", column, op, value);
        }

        public TQ WhereNotDate(string column, string op, object value)
        {
            return Not().WhereDate(column, op, value);
        }

        public TQ OrWhereDate(string column, string op, object value)
        {
            return Or().WhereDate(column, op, value);
        }

        public TQ OrWhereNotDate(string column, string op, object value)
        {
            return Or().Not().WhereDate(column, op, value);
        }

        public TQ WhereTime(string column, string op, object value)
        {
            return WhereDatePart("time", column, op, value);
        }

        public TQ WhereNotTime(string column, string op, object value)
        {
            return Not().WhereTime(column, op, value);
        }

        public TQ OrWhereTime(string column, string op, object value)
        {
            return Or().WhereTime(column, op, value);
        }

        public TQ OrWhereNotTime(string column, string op, object value)
        {
            return Or().Not().WhereTime(column, op, value);
        }

        public TQ WhereDatePart(string part, string column, object value)
        {
            return WhereDatePart(part, column, "=", value);
        }

        public TQ WhereNotDatePart(string part, string column, object value)
        {
            return WhereNotDatePart(part, column, "=", value);
        }

        public TQ OrWhereDatePart(string part, string column, object value)
        {
            return OrWhereDatePart(part, column, "=", value);
        }

        public TQ OrWhereNotDatePart(string part, string column, object value)
        {
            return OrWhereNotDatePart(part, column, "=", value);
        }

        public TQ WhereDate(string column, object value)
        {
            return WhereDate(column, "=", value);
        }

        public TQ WhereNotDate(string column, object value)
        {
            return WhereNotDate(column, "=", value);
        }

        public TQ OrWhereDate(string column, object value)
        {
            return OrWhereDate(column, "=", value);
        }

        public TQ OrWhereNotDate(string column, object value)
        {
            return OrWhereNotDate(column, "=", value);
        }

        public TQ WhereTime(string column, object value)
        {
            return WhereTime(column, "=", value);
        }

        public TQ WhereNotTime(string column, object value)
        {
            return WhereNotTime(column, "=", value);
        }

        public TQ OrWhereTime(string column, object value)
        {
            return OrWhereTime(column, "=", value);
        }

        public TQ OrWhereNotTime(string column, object value)
        {
            return OrWhereNotTime(column, "=", value);
        }

        #endregion
    }
}
