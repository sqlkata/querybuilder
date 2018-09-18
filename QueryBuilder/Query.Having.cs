using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SqlKata.Interfaces;

namespace SqlKata
{
    public partial class Query
    {
        public IQuery Having(string column, string op, object value)
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

        public IQuery HavingNot(string column, string op, object value)
        {
            return Not().Having(column, op, value);
        }

        public IQuery OrHaving(string column, string op, object value)
        {
            return Or().Having(column, op, value);
        }

        public IQuery OrHavingNot(string column, string op, object value)
        {
            return this.Or().Not().Having(column, op, value);
        }

        public IQuery Having(string column, object value)
        {
            return Having(column, "=", value);
        }
        public IQuery HavingNot(string column, object value)
        {
            return HavingNot(column, "=", value);
        }
        public IQuery OrHaving(string column, object value)
        {
            return OrHaving(column, "=", value);
        }
        public IQuery OrHavingNot(string column, object value)
        {
            return OrHavingNot(column, "=", value);
        }

        /// <summary>
        /// Perform a Having constraint
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public IQuery Having(object constraints)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var item in constraints.GetType().GetRuntimeProperties())
            {
                dictionary.Add(item.Name, item.GetValue(constraints));
            }

            return Having(dictionary);
        }

        public IQuery Having(IReadOnlyDictionary<string, object> values)
        {
            var query = (IQuery)this;
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
                    ((Query)query).And();
                }

                query = this.Not(notFlag).Having(tuple.Key, tuple.Value);
            }

            return query;
        }

        public IQuery HavingRaw(string sql, params object[] bindings)
        {
            return AddComponent("having", new RawCondition
            {
                Expression = sql,
                Bindings = Helper.Flatten(bindings).ToArray(),
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public IQuery OrHavingRaw(string sql, params object[] bindings)
        {
            return Or().HavingRaw(sql, bindings);
        }

        /// <summary>
        /// Apply a nested Having clause
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IQuery Having(Func<IQuery, IQuery> callback)
        {
            var query = callback.Invoke(NewChild());

            return AddComponent("having", new NestedCondition<IQuery>
            {
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr(),
            });
        }

        public IQuery HavingNot(Func<IQuery, IQuery> callback)
        {
            return Not().Having(callback);
        }

        public IQuery OrHaving(Func<IQuery, IQuery> callback)
        {
            return Or().Having(callback);
        }

        public IQuery OrHavingNot(Func<IQuery, IQuery> callback)
        {
            return Not().Or().Having(callback);
        }

        public IQuery HavingColumns(string first, string op, string second)
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

        public IQuery OrHavingColumns(string first, string op, string second)
        {
            return Or().HavingColumns(first, op, second);
        }

        public IQuery HavingNull(string column)
        {
            return AddComponent("having", new NullCondition
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public IQuery HavingNotNull(string column)
        {
            return Not().HavingNull(column);
        }

        public IQuery OrHavingNull(string column)
        {
            return this.Or().HavingNull(column);
        }

        public IQuery OrHavingNotNull(string column)
        {
            return Or().Not().HavingNull(column);
        }

        public IQuery HavingTrue(string column)
        {
            return AddComponent("having", new BooleanCondition
            {
                Column = column,
                Value = true,
            });
        }

        public IQuery OrHavingTrue(string column)
        {
            return Or().HavingTrue(column);
        }

        public IQuery HavingFalse(string column)
        {
            return AddComponent("having", new BooleanCondition
            {
                Column = column,
                Value = false,
            });
        }

        public IQuery OrHavingFalse(string column)
        {
            return Or().HavingFalse(column);
        }

        public IQuery HavingLike(string column, string value, bool caseSensitive = false)
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

        public IQuery HavingNotLike(string column, string value, bool caseSensitive = false)
        {
            return Not().HavingLike(column, value, caseSensitive);
        }

        public IQuery OrHavingLike(string column, string value, bool caseSensitive = false)
        {
            return Or().HavingLike(column, value, caseSensitive);
        }

        public IQuery OrHavingNotLike(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().HavingLike(column, value, caseSensitive);
        }
        public IQuery HavingStarts(string column, string value, bool caseSensitive = false)
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

        public IQuery HavingNotStarts(string column, string value, bool caseSensitive = false)
        {
            return Not().HavingStarts(column, value, caseSensitive);
        }

        public IQuery OrHavingStarts(string column, string value, bool caseSensitive = false)
        {
            return Or().HavingStarts(column, value, caseSensitive);
        }

        public IQuery OrHavingNotStarts(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().HavingStarts(column, value, caseSensitive);
        }

        public IQuery HavingEnds(string column, string value, bool caseSensitive = false)
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

        public IQuery HavingNotEnds(string column, string value, bool caseSensitive = false)
        {
            return Not().HavingEnds(column, value, caseSensitive);
        }

        public IQuery OrHavingEnds(string column, string value, bool caseSensitive = false)
        {
            return Or().HavingEnds(column, value, caseSensitive);
        }

        public IQuery OrHavingNotEnds(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().HavingEnds(column, value, caseSensitive);
        }

        public IQuery HavingContains(string column, string value, bool caseSensitive = false)
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

        public IQuery HavingNotContains(string column, string value, bool caseSensitive = false)
        {
            return Not().HavingContains(column, value, caseSensitive);
        }

        public IQuery OrHavingContains(string column, string value, bool caseSensitive = false)
        {
            return Or().HavingContains(column, value, caseSensitive);
        }

        public IQuery OrHavingNotContains(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().HavingContains(column, value, caseSensitive);
        }

        public IQuery HavingBetween<T>(string column, T lower, T higher)
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

        public IQuery OrHavingBetween<T>(string column, T lower, T higher)
        {
            return Or().HavingBetween(column, lower, higher);
        }
        public IQuery HavingNotBetween<T>(string column, T lower, T higher)
        {
            return Not().HavingBetween(column, lower, higher);
        }
        public IQuery OrHavingNotBetween<T>(string column, T lower, T higher)
        {
            return Or().Not().HavingBetween(column, lower, higher);
        }

        public IQuery HavingIn<T>(string column, IEnumerable<T> values)
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

        public IQuery OrHavingIn<T>(string column, IEnumerable<T> values)
        {
            return Or().HavingIn(column, values);
        }

        public IQuery HavingNotIn<T>(string column, IEnumerable<T> values)
        {
            return Not().HavingIn(column, values);
        }

        public IQuery OrHavingNotIn<T>(string column, IEnumerable<T> values)
        {
            return Or().Not().HavingIn(column, values);
        }


        public IQuery HavingIn(string column, IQuery query)
        {
            return AddComponent("having", new InQueryCondition
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Query = query,
            });
        }
        public IQuery HavingIn(string column, Func<IQuery, IQuery> callback)
        {
            var query = callback.Invoke(new Query());

            return HavingIn(column, query);
        }

        public IQuery OrHavingIn(string column, IQuery query)
        {
            return Or().HavingIn(column, query);
        }

        public IQuery OrHavingIn(string column, Func<IQuery, IQuery> callback)
        {
            return Or().HavingIn(column, callback);
        }
        public IQuery HavingNotIn(string column, IQuery query)
        {
            return Not().HavingIn(column, query);
        }

        public IQuery HavingNotIn(string column, Func<IQuery, IQuery> callback)
        {
            return Not().HavingIn(column, callback);
        }

        public IQuery OrHavingNotIn(string column, IQuery query)
        {
            return Or().Not().HavingIn(column, query);
        }

        public IQuery OrHavingNotIn(string column, Func<IQuery, IQuery> callback)
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
        public IQuery Having(string column, string op, Func<IQuery, IQuery> callback)
        {
            var query = callback.Invoke(NewChild());

            return Having(column, op, query);
        }

        public IQuery Having(string column, string op, IQuery query)
        {
            return AddComponent("having", new QueryCondition<IQuery>
            {
                Column = column,
                Operator = op,
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr(),
            });
        }

        public IQuery OrHaving(string column, string op, IQuery query)
        {
            return Or().Having(column, op, query);
        }
        public IQuery OrHaving(string column, string op, Func<IQuery, IQuery> callback)
        {
            return Or().Having(column, op, callback);
        }

        public IQuery HavingExists(IQuery query)
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
        public IQuery HavingExists(Func<IQuery, IQuery> callback)
        {
            var childQuery = new Query().SetParent(this);
            return HavingExists(callback.Invoke(childQuery));
        }

        public IQuery HavingNotExists(IQuery query)
        {
            return Not().HavingExists(query);
        }

        public IQuery HavingNotExists(Func<IQuery, IQuery> callback)
        {
            return Not().HavingExists(callback);
        }

        public IQuery OrHavingExists(IQuery query)
        {
            return Or().HavingExists(query);
        }
        public IQuery OrHavingExists(Func<IQuery, IQuery> callback)
        {
            return Or().HavingExists(callback);
        }
        public IQuery OrHavingNotExists(IQuery query)
        {
            return Or().Not().HavingExists(query);
        }
        public IQuery OrHavingNotExists(Func<IQuery, IQuery> callback)
        {
            return Or().Not().HavingExists(callback);
        }

        #region date
        public IQuery HavingDatePart(string part, string column, string op, object value)
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
        public IQuery HavingNotDatePart(string part, string column, string op, object value)
        {
            return Not().HavingDatePart(part, column, op, value);
        }

        public IQuery OrHavingDatePart(string part, string column, string op, object value)
        {
            return Or().HavingDatePart(part, column, op, value);
        }

        public IQuery OrHavingNotDatePart(string part, string column, string op, object value)
        {
            return Or().Not().HavingDatePart(part, column, op, value);
        }

        public IQuery HavingDate(string column, string op, object value)
        {
            return HavingDatePart("date", column, op, value);
        }
        public IQuery HavingNotDate(string column, string op, object value)
        {
            return Not().HavingDate(column, op, value);
        }
        public IQuery OrHavingDate(string column, string op, object value)
        {
            return Or().HavingDate(column, op, value);
        }
        public IQuery OrHavingNotDate(string column, string op, object value)
        {
            return Or().Not().HavingDate(column, op, value);
        }

        public IQuery HavingTime(string column, string op, object value)
        {
            return HavingDatePart("time", column, op, value);
        }
        public IQuery HavingNotTime(string column, string op, object value)
        {
            return Not().HavingTime(column, op, value);
        }
        public IQuery OrHavingTime(string column, string op, object value)
        {
            return Or().HavingTime(column, op, value);
        }
        public IQuery OrHavingNotTime(string column, string op, object value)
        {
            return Or().Not().HavingTime(column, op, value);
        }

        public IQuery HavingDatePart(string part, string column, object value)
        {
            return HavingDatePart(part, column, "=", value);
        }
        public IQuery HavingNotDatePart(string part, string column, object value)
        {
            return HavingNotDatePart(part, column, "=", value);
        }

        public IQuery OrHavingDatePart(string part, string column, object value)
        {
            return OrHavingDatePart(part, column, "=", value);
        }

        public IQuery OrHavingNotDatePart(string part, string column, object value)
        {
            return OrHavingNotDatePart(part, column, "=", value);
        }

        public IQuery HavingDate(string column, object value)
        {
            return HavingDate(column, "=", value);
        }
        public IQuery HavingNotDate(string column, object value)
        {
            return HavingNotDate(column, "=", value);
        }
        public IQuery OrHavingDate(string column, object value)
        {
            return OrHavingDate(column, "=", value);
        }
        public IQuery OrHavingNotDate(string column, object value)
        {
            return OrHavingNotDate(column, "=", value);
        }

        public IQuery HavingTime(string column, object value)
        {
            return HavingTime(column, "=", value);
        }
        public IQuery HavingNotTime(string column, object value)
        {
            return HavingNotTime(column, "=", value);
        }
        public IQuery OrHavingTime(string column, object value)
        {
            return OrHavingTime(column, "=", value);
        }
        public IQuery OrHavingNotTime(string column, object value)
        {
            return OrHavingNotTime(column, "=", value);
        }

        #endregion
    }
}