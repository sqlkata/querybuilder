using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata
{
    public static class ConditionExtensions
    {
        public static T Or<T>(this T target) where T : IConditional
        {
            target.SetOr(true);
            return target;
        }

        public static T Not<T>(this T target, bool flag = true) where T : IConditional
        {
            target.SetNot(flag);
            return target;
        }

        public static T And<T>(this T target) where T : IConditional
        {
            target.SetOr(false);
            return target;
        }

        public static T Where<T>(this T target, string column, string op, object value) where T : IConditional
        {
            if (value == null)
            {
                return target.Not(op != "=").WhereNull(column);
            }

            if (value is bool boolValue)
            {
                if (op != "=")
                {
                    target.Not();
                }

                return boolValue ? target.WhereTrue(column) : target.WhereFalse(column);
            }

            target.AddComponent("where", new BasicCondition
            {
                Column = column,
                Operator = op,
                Value = value,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
            });

            return target;
        }

        public static T WhereNot<T>(this T target, string column, string op, object value) where T : IConditional
        {
            return target.Not().Where(column, op, value);
        }

        public static T OrWhere<T>(this T target, string column, string op, object value) where T : IConditional
        {
            return target.Or().Where(column, op, value);
        }

        public static T OrWhereNot<T>(this T target, string column, string op, object value) where T : IConditional
        {
            return target.Or().Not().Where(column, op, value);
        }

        public static T Where<T>(this T target, string column, object value) where T : IConditional
        {
            return target.Where(column, "=", value);
        }

        public static T WhereNot<T>(this T target, string column, object value) where T : IConditional
        {
            return target.WhereNot(column, "=", value);
        }

        public static T OrWhere<T>(this T target, string column, object value) where T : IConditional
        {
            return target.OrWhere(column, "=", value);
        }

        public static T OrWhereNot<T>(this T target, string column, object value) where T : IConditional
        {
            return target.OrWhereNot(column, "=", value);
        }

        public static T Where<T>(this T target, object constraints) where T : IConditional
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var item in constraints.GetType().GetRuntimeProperties())
            {
                dictionary.Add(item.Name, item.GetValue(constraints));
            }

            return target.Where(dictionary);
        }

        public static T Where<T>(this T target, IEnumerable<KeyValuePair<string, object>> values) where T : IConditional
        {
            var orFlag = target.GetOr();
            var notFlag = target.GetNot();

            foreach (var tuple in values)
            {
                if (orFlag)
                {
                    target.Or();
                }
                else
                {
                    target.And();
                }

                target.Not(notFlag).Where(tuple.Key, tuple.Value);
            }

            return target;
        }

        public static T WhereRaw<T>(this T target, string sql, params object[] bindings) where T : IConditional
        {
            target.AddComponent("where", new RawCondition
            {
                Expression = sql,
                Bindings = bindings,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
            });

            return target;
        }

        public static T OrWhereRaw<T>(this T target, string sql, params object[] bindings) where T : IConditional
        {
            return target.Or().WhereRaw(sql, bindings);
        }

        public static T Where<T>(this T target, Func<T, T> callback) where T : IConditional
        {
            var child = (T)target.NewChild();
            var query = callback.Invoke(child);

            // omit empty queries
            if (!query.HasComponent("where"))
            {
                return target;
            }

            target.AddComponent("where", new NestedCondition
            {
                Conditions = query,
                IsNot = target.GetNot(),
                IsOr = target.GetOr(),
            });

            return target;
        }

        public static T WhereNot<T>(this T target, Func<T, T> callback) where T : IConditional
        {
            return target.Not().Where(callback);
        }

        public static T OrWhere<T>(this T target, Func<T, T> callback) where T : IConditional
        {
            return target.Or().Where(callback);
        }

        public static T OrWhereNot<T>(this T target, Func<T, T> callback) where T : IConditional
        {
            return target.Not().Or().Where(callback);
        }

        public static T WhereColumns<T>(this T target, string first, string op, string second) where T : IConditional
        {
            target.AddComponent("where", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
            });

            return target;
        }

        public static T OrWhereColumns<T>(this T target, string first, string op, string second) where T : IConditional
        {
            return target.Or().WhereColumns(first, op, second);
        }

        public static T WhereNull<T>(this T target, string column) where T : IConditional
        {
            target.AddComponent("where", new NullCondition
            {
                Column = column,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
            });

            return target;
        }

        public static T WhereNotNull<T>(this T target, string column) where T : IConditional
        {
            return target.Not().WhereNull(column);
        }

        public static T OrWhereNull<T>(this T target, string column) where T : IConditional
        {
            return target.Or().WhereNull(column);
        }

        public static T OrWhereNotNull<T>(this T target, string column) where T : IConditional
        {
            return target.Or().Not().WhereNull(column);
        }

        public static T WhereTrue<T>(this T target, string column) where T : IConditional
        {
            target.AddComponent("where", new BooleanCondition
            {
                Column = column,
                Value = true,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
            });

            return target;
        }

        public static T OrWhereTrue<T>(this T target, string column) where T : IConditional
        {
            return target.Or().WhereTrue(column);
        }

        public static T WhereFalse<T>(this T target, string column) where T : IConditional
        {
            target.AddComponent("where", new BooleanCondition
            {
                Column = column,
                Value = false,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
            });

            return target;
        }

        public static T OrWhereFalse<T>(this T target, string column) where T : IConditional
        {
            return target.Or().WhereFalse(column);
        }

        public static T WhereLike<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            target.AddComponent("where", new BasicStringCondition
            {
                Operator = "like",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
            });

            return target;
        }

        public static T WhereNotLike<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Not().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public static T OrWhereLike<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Or().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public static T OrWhereNotLike<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Or().Not().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public static T WhereStarts<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            target.AddComponent("where", new BasicStringCondition
            {
                Operator = "starts",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
            });

            return target;
        }

        public static T WhereNotStarts<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Not().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public static T OrWhereStarts<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Or().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public static T OrWhereNotStarts<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Or().Not().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public static T WhereEnds<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            target.AddComponent("where", new BasicStringCondition
            {
                Operator = "ends",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
            });

            return target;
        }

        public static T WhereNotEnds<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Not().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public static T OrWhereEnds<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Or().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public static T OrWhereNotEnds<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Or().Not().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public static T WhereContains<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            target.AddComponent("where", new BasicStringCondition
            {
                Operator = "contains",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
            });

            return target;
        }

        public static T WhereNotContains<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Not().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public static T OrWhereContains<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Or().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public static T OrWhereNotContains<T>(this T target, string column, object value, bool caseSensitive = false, string escapeCharacter = null) where T : IConditional
        {
            return target.Or().Not().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public static T WhereBetween<T, TVal>(this T target, string column, TVal lower, TVal higher) where T : IConditional
        {
            target.AddComponent("where", new BetweenCondition
            {
                Column = column,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
                Lower = lower,
                Higher = higher
            });

            return target;
        }

        public static T OrWhereBetween<T, TVal>(this T target, string column, TVal lower, TVal higher) where T : IConditional
        {
            return target.Or().WhereBetween(column, lower, higher);
        }

        public static T WhereNotBetween<T, TVal>(this T target, string column, TVal lower, TVal higher) where T : IConditional
        {
            return target.Not().WhereBetween(column, lower, higher);
        }

        public static T OrWhereNotBetween<T, TVal>(this T target, string column, TVal lower, TVal higher) where T : IConditional
        {
            return target.Or().Not().WhereBetween(column, lower, higher);
        }

        public static T WhereIn<T, TVal>(this T target, string column, IEnumerable<TVal> values) where T : IConditional
        {
            if (values is string val)
            {
                target.AddComponent("where", new InCondition
                {
                    Column = column,
                    IsOr = target.GetOr(),
                    IsNot = target.GetNot(),
                    Values = new List<object> { val }
                });

                return target;
            }

            target.AddComponent("where", new InCondition
            {
                Column = column,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
                Values = values.Distinct().Cast<object>().ToList()
            });

            return target;
        }

        public static T OrWhereIn<T, TVal>(this T target, string column, IEnumerable<TVal> values) where T : IConditional
        {
            return target.Or().WhereIn(column, values);
        }

        public static T WhereNotIn<T, TVal>(this T target, string column, IEnumerable<TVal> values) where T : IConditional
        {
            return target.Not().WhereIn(column, values);
        }

        public static T OrWhereNotIn<T, TVal>(this T target, string column, IEnumerable<TVal> values) where T : IConditional
        {
            return target.Or().Not().WhereIn(column, values);
        }

        public static T WhereIn<T>(this T target, string column, Query query) where T : IConditional
        {
            target.AddComponent("where", new InQueryCondition
            {
                Column = column,
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
                Query = query,
            });

            return target;
        }

        public static T WhereIn<T>(this T target, string column, Func<Query, Query> callback) where T : IConditional
        {
            return target.WhereIn(column, callback.Invoke(new Query()));
        }

        public static T OrWhereIn<T>(this T target, string column, Query query) where T : IConditional
        {
            return target.Or().WhereIn(column, query);
        }

        public static T OrWhereIn<T>(this T target, string column, Func<Query, Query> callback) where T : IConditional
        {
            return target.Or().WhereIn(column, callback);
        }

        public static T WhereNotIn<T>(this T target, string column, Query query) where T : IConditional
        {
            return target.Not().WhereIn(column, query);
        }

        public static T WhereNotIn<T>(this T target, string column, Func<Query, Query> callback) where T : IConditional
        {
            return target.Not().WhereIn(column, callback);
        }

        public static T OrWhereNotIn<T>(this T target, string column, Query query) where T : IConditional
        {
            return target.Or().Not().WhereIn(column, query);
        }

        public static T OrWhereNotIn<T>(this T target, string column, Func<Query, Query> callback) where T : IConditional
        {
            return target.Or().Not().WhereIn(column, callback);
        }

        public static T Where<T>(this T target, string column, string op, Query query) where T : IConditional
        {
            target.AddComponent("where", new QueryCondition
            {
                Column = column,
                Operator = op,
                Query = query,
                IsNot = target.GetNot(),
                IsOr = target.GetOr(),
            });

            return target;
        }

        public static T Where<T>(this T target, string column, string op, Func<Query, Query> callback) where T : IConditional
        {
            return target.Where(column, op, callback.Invoke(new Query()));
        }

        public static T WhereSub<T>(this T target, Query query, object value) where T : IConditional
        {
            return target.WhereSub(query, "=", value);
        }

        public static T WhereSub<T>(this T target, Query query, string op, object value) where T : IConditional
        {
            target.AddComponent("where", new SubQueryCondition
            {
                Value = value,
                Operator = op,
                Query = query,
                IsNot = target.GetNot(),
                IsOr = target.GetOr(),
            });

            return target;
        }

        public static T OrWhereSub<T>(this T target, Query query, object value) where T : IConditional
        {
            return target.Or().WhereSub(query, value);
        }

        public static T OrWhereSub<T>(this T target, Query query, string op, object value) where T : IConditional
        {
            return target.Or().WhereSub(query, op, value);
        }

        public static T OrWhere<T>(this T target, string column, string op, Query query) where T : IConditional
        {
            return target.Or().Where(column, op, query);
        }

        public static T OrWhere<T>(this T target, string column, string op, Func<Query, Query> callback) where T : IConditional
        {
            return target.Or().Where(column, op, callback);
        }

        public static T WhereExists<T>(this T target, Query query) where T : IConditional
        {
            if (!query.FromClauses.Any())
            {
                throw new ArgumentException($"'{nameof(FromClause)}' cannot be empty if used inside a '{nameof(WhereExists)}' condition");
            }

            target.AddComponent("where", new ExistsCondition
            {
                Query = query,
                IsNot = target.GetNot(),
                IsOr = target.GetOr(),
            });

            return target;
        }

        public static T WhereExists<T>(this T target, Func<Query, Query> callback) where T : IConditional
        {
            return target.WhereExists(callback.Invoke(new Query()));
        }

        public static T WhereNotExists<T>(this T target, Query query) where T : IConditional
        {
            return target.Not().WhereExists(query);
        }

        public static T WhereNotExists<T>(this T target, Func<Query, Query> callback) where T : IConditional
        {
            return target.Not().WhereExists(callback);
        }

        public static T OrWhereExists<T>(this T target, Query query) where T : IConditional
        {
            return target.Or().WhereExists(query);
        }

        public static T OrWhereExists<T>(this T target, Func<Query, Query> callback) where T : IConditional
        {
            return target.Or().WhereExists(callback);
        }

        public static T OrWhereNotExists<T>(this T target, Query query) where T : IConditional
        {
            return target.Or().Not().WhereExists(query);
        }

        public static T OrWhereNotExists<T>(this T target, Func<Query, Query> callback) where T : IConditional
        {
            return target.Or().Not().WhereExists(callback);
        }

        #region date
        public static T WhereDatePart<T>(this T target, string part, string column, string op, object value) where T : IConditional
        {
            target.AddComponent("where", new BasicDateCondition
            {
                Operator = op,
                Column = column,
                Value = value,
                Part = part?.ToLowerInvariant(),
                IsOr = target.GetOr(),
                IsNot = target.GetNot(),
            });

            return target;
        }

        public static T WhereNotDatePart<T>(this T target, string part, string column, string op, object value) where T : IConditional
        {
            return target.Not().WhereDatePart(part, column, op, value);
        }

        public static T OrWhereDatePart<T>(this T target, string part, string column, string op, object value) where T : IConditional
        {
            return target.Or().WhereDatePart(part, column, op, value);
        }

        public static T OrWhereNotDatePart<T>(this T target, string part, string column, string op, object value) where T : IConditional
        {
            return target.Or().Not().WhereDatePart(part, column, op, value);
        }

        public static T WhereDate<T>(this T target, string column, string op, object value) where T : IConditional
        {
            return target.WhereDatePart("date", column, op, value);
        }

        public static T WhereNotDate<T>(this T target, string column, string op, object value) where T : IConditional
        {
            return target.Not().WhereDate(column, op, value);
        }

        public static T OrWhereDate<T>(this T target, string column, string op, object value) where T : IConditional
        {
            return target.Or().WhereDate(column, op, value);
        }

        public static T OrWhereNotDate<T>(this T target, string column, string op, object value) where T : IConditional
        {
            return target.Or().Not().WhereDate(column, op, value);
        }

        public static T WhereTime<T>(this T target, string column, string op, object value) where T : IConditional
        {
            return target.WhereDatePart("time", column, op, value);
        }

        public static T WhereNotTime<T>(this T target, string column, string op, object value) where T : IConditional
        {
            return target.Not().WhereTime(column, op, value);
        }

        public static T OrWhereTime<T>(this T target, string column, string op, object value) where T : IConditional
        {
            return target.Or().WhereTime(column, op, value);
        }

        public static T OrWhereNotTime<T>(this T target, string column, string op, object value) where T : IConditional
        {
            return target.Or().Not().WhereTime(column, op, value);
        }

        public static T WhereDatePart<T>(this T target, string part, string column, object value) where T : IConditional
        {
            return target.WhereDatePart(part, column, "=", value);
        }

        public static T WhereNotDatePart<T>(this T target, string part, string column, object value) where T : IConditional
        {
            return target.WhereNotDatePart(part, column, "=", value);
        }

        public static T OrWhereDatePart<T>(this T target, string part, string column, object value) where T : IConditional
        {
            return target.OrWhereDatePart(part, column, "=", value);
        }

        public static T OrWhereNotDatePart<T>(this T target, string part, string column, object value) where T : IConditional
        {
            return target.OrWhereNotDatePart(part, column, "=", value);
        }

        public static T WhereDate<T>(this T target, string column, object value) where T : IConditional
        {
            return target.WhereDate(column, "=", value);
        }

        public static T WhereNotDate<T>(this T target, string column, object value) where T : IConditional
        {
            return target.WhereNotDate(column, "=", value);
        }

        public static T OrWhereDate<T>(this T target, string column, object value) where T : IConditional
        {
            return target.OrWhereDate(column, "=", value);
        }

        public static T OrWhereNotDate<T>(this T target, string column, object value) where T : IConditional
        {
            return target.OrWhereNotDate(column, "=", value);
        }

        public static T WhereTime<T>(this T target, string column, object value) where T : IConditional
        {
            return target.WhereTime(column, "=", value);
        }

        public static T WhereNotTime<T>(this T target, string column, object value) where T : IConditional
        {
            return target.WhereNotTime(column, "=", value);
        }

        public static T OrWhereTime<T>(this T target, string column, object value) where T : IConditional
        {
            return target.OrWhereTime(column, "=", value);
        }

        public static T OrWhereNotTime<T>(this T target, string column, object value) where T : IConditional
        {
            return target.OrWhereNotTime(column, "=", value);
        }
        #endregion
    }
}
