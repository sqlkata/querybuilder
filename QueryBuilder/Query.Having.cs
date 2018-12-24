using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata
{
    public partial class Query
    {
        public Query Having(string column, string op, object value)
        {

            // If the value is "null", we will just assume the developer wants to add a
            // Having null clause to the query. So, we will allow a short-cut here to
            // that method for convenience so the developer doesn't have to check.
            if (value == null)
            {
                return Not(op != "=").HavingNull(column);
            }

            return AddComponent("having", new BasicCondition
            {
                Column = column,
                Operator = op,
                Value = value,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Query HavingNot(string column, string op, object value)
        {
            return Not().Having(column, op, value);
        }

        public Query OrHaving(string column, string op, object value)
        {
            return Or().Having(column, op, value);
        }

        public Query OrHavingNot(string column, string op, object value)
        {
            return this.Or().Not().Having(column, op, value);
        }

        public Query Having(string column, object value)
        {
            return Having(column, "=", value);
        }
        public Query HavingNot(string column, object value)
        {
            return HavingNot(column, "=", value);
        }
        public Query OrHaving(string column, object value)
        {
            return OrHaving(column, "=", value);
        }
        public Query OrHavingNot(string column, object value)
        {
            return OrHavingNot(column, "=", value);
        }

        /// <summary>
        /// Perform a Having constraint
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public Query Having(object constraints)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var item in constraints.GetType().GetRuntimeProperties())
            {
                dictionary.Add(item.Name, item.GetValue(constraints));
            }

            return Having(dictionary);
        }

        public Query Having(IReadOnlyDictionary<string, object> values)
        {
            var query = this;
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

                query = this.Not(notFlag).Having(tuple.Key, tuple.Value);
            }

            return query;
        }

        public Query HavingRaw(string sql, params object[] bindings)
        {
            return AddComponent("having", new RawCondition
            {
                Expression = sql,
                Bindings = bindings,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Query OrHavingRaw(string sql, params object[] bindings)
        {
            return Or().HavingRaw(sql, bindings);
        }

        /// <summary>
        /// Apply a nested Having clause
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Query Having(Func<Query, Query> callback)
        {
            var query = callback.Invoke(NewChild());

            return AddComponent("having", new NestedCondition<Query>
            {
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr(),
            });
        }

        public Query HavingNot(Func<Query, Query> callback)
        {
            return Not().Having(callback);
        }

        public Query OrHaving(Func<Query, Query> callback)
        {
            return Or().Having(callback);
        }

        public Query OrHavingNot(Func<Query, Query> callback)
        {
            return Not().Or().Having(callback);
        }

        public Query HavingColumns(string first, string op, string second)
        {
            return AddComponent("having", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Query OrHavingColumns(string first, string op, string second)
        {
            return Or().HavingColumns(first, op, second);
        }

        public Query HavingNull(string column)
        {
            return AddComponent("having", new NullCondition
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Query HavingNotNull(string column)
        {
            return Not().HavingNull(column);
        }

        public Query OrHavingNull(string column)
        {
            return this.Or().HavingNull(column);
        }

        public Query OrHavingNotNull(string column)
        {
            return Or().Not().HavingNull(column);
        }

        public Query HavingTrue(string column)
        {
            return AddComponent("having", new BooleanCondition
            {
                Column = column,
                Value = true,
            });
        }

        public Query OrHavingTrue(string column)
        {
            return Or().HavingTrue(column);
        }

        public Query HavingFalse(string column)
        {
            return AddComponent("having", new BooleanCondition
            {
                Column = column,
                Value = false,
            });
        }

        public Query OrHavingFalse(string column)
        {
            return Or().HavingFalse(column);
        }

        public Query HavingLike(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("having", new BasicStringCondition
            {
                Operator = "like",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Query HavingNotLike(string column, string value, bool caseSensitive = false)
        {
            return Not().HavingLike(column, value, caseSensitive);
        }

        public Query OrHavingLike(string column, string value, bool caseSensitive = false)
        {
            return Or().HavingLike(column, value, caseSensitive);
        }

        public Query OrHavingNotLike(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().HavingLike(column, value, caseSensitive);
        }
        public Query HavingStarts(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("having", new BasicStringCondition
            {
                Operator = "starts",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Query HavingNotStarts(string column, string value, bool caseSensitive = false)
        {
            return Not().HavingStarts(column, value, caseSensitive);
        }

        public Query OrHavingStarts(string column, string value, bool caseSensitive = false)
        {
            return Or().HavingStarts(column, value, caseSensitive);
        }

        public Query OrHavingNotStarts(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().HavingStarts(column, value, caseSensitive);
        }

        public Query HavingEnds(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("having", new BasicStringCondition
            {
                Operator = "ends",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Query HavingNotEnds(string column, string value, bool caseSensitive = false)
        {
            return Not().HavingEnds(column, value, caseSensitive);
        }

        public Query OrHavingEnds(string column, string value, bool caseSensitive = false)
        {
            return Or().HavingEnds(column, value, caseSensitive);
        }

        public Query OrHavingNotEnds(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().HavingEnds(column, value, caseSensitive);
        }

        public Query HavingContains(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("having", new BasicStringCondition
            {
                Operator = "contains",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Query HavingNotContains(string column, string value, bool caseSensitive = false)
        {
            return Not().HavingContains(column, value, caseSensitive);
        }

        public Query OrHavingContains(string column, string value, bool caseSensitive = false)
        {
            return Or().HavingContains(column, value, caseSensitive);
        }

        public Query OrHavingNotContains(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().HavingContains(column, value, caseSensitive);
        }

        public Query HavingBetween<T>(string column, T lower, T higher)
        {
            return AddComponent("having", new BetweenCondition<T>
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Lower = lower,
                Higher = higher
            });
        }

        public Query OrHavingBetween<T>(string column, T lower, T higher)
        {
            return Or().HavingBetween(column, lower, higher);
        }
        public Query HavingNotBetween<T>(string column, T lower, T higher)
        {
            return Not().HavingBetween(column, lower, higher);
        }
        public Query OrHavingNotBetween<T>(string column, T lower, T higher)
        {
            return Or().Not().HavingBetween(column, lower, higher);
        }

        public Query HavingIn<T>(string column, IEnumerable<T> values)
        {
            // If the developer has passed a string most probably he wants List<string>
            // since string is considered as List<char>
            if (values is string)
            {
                string val = values as string;

                return AddComponent("having", new InCondition<string>
                {
                    Column = column,
                    IsOr = GetOr(),
                    IsNot = GetNot(),
                    Values = new List<string> { val }
                });
            }

            return AddComponent("having", new InCondition<T>
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Values = values.Distinct().ToList()
            });


        }

        public Query OrHavingIn<T>(string column, IEnumerable<T> values)
        {
            return Or().HavingIn(column, values);
        }

        public Query HavingNotIn<T>(string column, IEnumerable<T> values)
        {
            return Not().HavingIn(column, values);
        }

        public Query OrHavingNotIn<T>(string column, IEnumerable<T> values)
        {
            return Or().Not().HavingIn(column, values);
        }


        public Query HavingIn(string column, Query query)
        {
            return AddComponent("having", new InQueryCondition
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Query = query,
            });
        }
        public Query HavingIn(string column, Func<Query, Query> callback)
        {
            var query = callback.Invoke(new Query());

            return HavingIn(column, query);
        }

        public Query OrHavingIn(string column, Query query)
        {
            return Or().HavingIn(column, query);
        }

        public Query OrHavingIn(string column, Func<Query, Query> callback)
        {
            return Or().HavingIn(column, callback);
        }
        public Query HavingNotIn(string column, Query query)
        {
            return Not().HavingIn(column, query);
        }

        public Query HavingNotIn(string column, Func<Query, Query> callback)
        {
            return Not().HavingIn(column, callback);
        }

        public Query OrHavingNotIn(string column, Query query)
        {
            return Or().Not().HavingIn(column, query);
        }

        public Query OrHavingNotIn(string column, Func<Query, Query> callback)
        {
            return Or().Not().HavingIn(column, callback);
        }


        /// <summary>
        /// Perform a sub query Having clause
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Query Having(string column, string op, Func<Query, Query> callback)
        {
            var query = callback.Invoke(NewChild());

            return Having(column, op, query);
        }

        public Query Having(string column, string op, Query query)
        {
            return AddComponent("having", new QueryCondition<Query>
            {
                Column = column,
                Operator = op,
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr(),
            });
        }

        public Query OrHaving(string column, string op, Query query)
        {
            return Or().Having(column, op, query);
        }
        public Query OrHaving(string column, string op, Func<Query, Query> callback)
        {
            return Or().Having(column, op, callback);
        }

        public Query HavingExists(Query query)
        {
            if (!query.HasComponent("from"))
            {
                throw new ArgumentException("'FromClause' cannot be empty if used inside a 'HavingExists' condition");
            }

            // simplify the query as much as possible
            query = query.Clone().ClearComponent("select")
                .SelectRaw("1")
                .Limit(1);

            return AddComponent("having", new ExistsCondition
            {
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr(),
            });
        }
        public Query HavingExists(Func<Query, Query> callback)
        {
            var childQuery = new Query().SetParent(this);
            return HavingExists(callback.Invoke(childQuery));
        }

        public Query HavingNotExists(Query query)
        {
            return Not().HavingExists(query);
        }

        public Query HavingNotExists(Func<Query, Query> callback)
        {
            return Not().HavingExists(callback);
        }

        public Query OrHavingExists(Query query)
        {
            return Or().HavingExists(query);
        }
        public Query OrHavingExists(Func<Query, Query> callback)
        {
            return Or().HavingExists(callback);
        }
        public Query OrHavingNotExists(Query query)
        {
            return Or().Not().HavingExists(query);
        }
        public Query OrHavingNotExists(Func<Query, Query> callback)
        {
            return Or().Not().HavingExists(callback);
        }

        #region date
        public Query HavingDatePart(string part, string column, string op, object value)
        {
            return AddComponent("having", new BasicDateCondition
            {
                Operator = op,
                Column = column,
                Value = value,
                Part = part,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }
        public Query HavingNotDatePart(string part, string column, string op, object value)
        {
            return Not().HavingDatePart(part, column, op, value);
        }

        public Query OrHavingDatePart(string part, string column, string op, object value)
        {
            return Or().HavingDatePart(part, column, op, value);
        }

        public Query OrHavingNotDatePart(string part, string column, string op, object value)
        {
            return Or().Not().HavingDatePart(part, column, op, value);
        }

        public Query HavingDate(string column, string op, object value)
        {
            return HavingDatePart("date", column, op, value);
        }
        public Query HavingNotDate(string column, string op, object value)
        {
            return Not().HavingDate(column, op, value);
        }
        public Query OrHavingDate(string column, string op, object value)
        {
            return Or().HavingDate(column, op, value);
        }
        public Query OrHavingNotDate(string column, string op, object value)
        {
            return Or().Not().HavingDate(column, op, value);
        }

        public Query HavingTime(string column, string op, object value)
        {
            return HavingDatePart("time", column, op, value);
        }
        public Query HavingNotTime(string column, string op, object value)
        {
            return Not().HavingTime(column, op, value);
        }
        public Query OrHavingTime(string column, string op, object value)
        {
            return Or().HavingTime(column, op, value);
        }
        public Query OrHavingNotTime(string column, string op, object value)
        {
            return Or().Not().HavingTime(column, op, value);
        }

        public Query HavingDatePart(string part, string column, object value)
        {
            return HavingDatePart(part, column, "=", value);
        }
        public Query HavingNotDatePart(string part, string column, object value)
        {
            return HavingNotDatePart(part, column, "=", value);
        }

        public Query OrHavingDatePart(string part, string column, object value)
        {
            return OrHavingDatePart(part, column, "=", value);
        }

        public Query OrHavingNotDatePart(string part, string column, object value)
        {
            return OrHavingNotDatePart(part, column, "=", value);
        }

        public Query HavingDate(string column, object value)
        {
            return HavingDate(column, "=", value);
        }
        public Query HavingNotDate(string column, object value)
        {
            return HavingNotDate(column, "=", value);
        }
        public Query OrHavingDate(string column, object value)
        {
            return OrHavingDate(column, "=", value);
        }
        public Query OrHavingNotDate(string column, object value)
        {
            return OrHavingNotDate(column, "=", value);
        }

        public Query HavingTime(string column, object value)
        {
            return HavingTime(column, "=", value);
        }
        public Query HavingNotTime(string column, object value)
        {
            return HavingNotTime(column, "=", value);
        }
        public Query OrHavingTime(string column, object value)
        {
            return OrHavingTime(column, "=", value);
        }
        public Query OrHavingNotTime(string column, object value)
        {
            return OrHavingNotTime(column, "=", value);
        }

        #endregion
    }
}