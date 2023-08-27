using System.Collections.Immutable;
using System.Reflection;

namespace SqlKata
{
    public partial class Query
    {
        public Query Having(string column, string op, object? value)
        {
            // If the value is "null", we will just assume the developer wants to add a
            // Having null clause to the query. So, we will allow a short-cut here to
            // that method for convenience so the developer doesn't have to check.
            if (value == null) return Not(op != "=").HavingNull(column);

            return AddComponent(new BasicCondition
            {
                Engine = EngineScope,
                Component = "having",
                Column = column,
                Operator = op,
                Value = value,
                IsOr = GetOr(),
                IsNot = GetNot()
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
            return Or().Not().Having(column, op, value);
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
        ///     Perform a Having constraint
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public Query Having(object constraints)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var item in constraints.GetType().GetRuntimeProperties())
                dictionary.Add(item.Name, item.GetValue(constraints));

            return Having(dictionary);
        }

        public Query Having(IEnumerable<KeyValuePair<string, object>> values)
        {
            var query = this;
            var orFlag = GetOr();
            var notFlag = GetNot();

            foreach (var tuple in values)
            {
                if (orFlag)
                    query.Or();
                else
                    query.And();

                query = Not(notFlag).Having(tuple.Key, tuple.Value);
            }

            return query;
        }

        public Query HavingRaw(string sql, params object[] bindings)
        {
            return AddComponent(new RawCondition
            {
                Engine = EngineScope,
                Component = "having",
                Expression = sql,
                Bindings = bindings,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query OrHavingRaw(string sql, params object[] bindings)
        {
            return Or().HavingRaw(sql, bindings);
        }

        /// <summary>
        ///     Apply a nested Having clause
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Query Having(Func<Query, Query> callback)
        {
            var query = callback.Invoke(NewChild());

            return AddComponent(new NestedCondition
            {
                Engine = EngineScope,
                Component = "having",
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
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
            return AddComponent(new TwoColumnsCondition
            {
                Engine = EngineScope,
                Component = "having",
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query OrHavingColumns(string first, string op, string second)
        {
            return Or().HavingColumns(first, op, second);
        }

        public Query HavingNull(string column)
        {
            return AddComponent(new NullCondition
            {
                Engine = EngineScope,
                Component = "having",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query HavingNotNull(string column)
        {
            return Not().HavingNull(column);
        }

        public Query OrHavingNull(string column)
        {
            return Or().HavingNull(column);
        }

        public Query OrHavingNotNull(string column)
        {
            return Or().Not().HavingNull(column);
        }

        public Query HavingTrue(string column)
        {
            return AddComponent(new BooleanCondition
            {
                IsOr = false,
                IsNot = false,
                Engine = EngineScope,
                Component = "having",
                Column = column,
                Value = true
            });
        }

        public Query OrHavingTrue(string column)
        {
            return Or().HavingTrue(column);
        }

        public Query HavingFalse(string column)
        {
            return AddComponent(new BooleanCondition
            {
                IsOr = false,
                IsNot = false,
                Engine = EngineScope,
                Component = "having",
                Column = column,
                Value = false
            });
        }

        public Query OrHavingFalse(string column)
        {
            return Or().HavingFalse(column);
        }

        public Query HavingLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return AddComponent(new BasicStringCondition
            {
                Engine = EngineScope,
                Component = "having",
                Operator = "like",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query HavingNotLike(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Not().HavingLike(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrHavingLike(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Or().HavingLike(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrHavingNotLike(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Or().Not().HavingLike(column, value, caseSensitive, escapeCharacter);
        }

        public Query HavingStarts(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return AddComponent(new BasicStringCondition
            {
                Engine = EngineScope,
                Component = "having",
                Operator = "starts",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query HavingNotStarts(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Not().HavingStarts(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrHavingStarts(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Or().HavingStarts(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrHavingNotStarts(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Or().Not().HavingStarts(column, value, caseSensitive, escapeCharacter);
        }

        public Query HavingEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            return AddComponent(new BasicStringCondition
            {
                Engine = EngineScope,
                Component = "having",
                Operator = "ends",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query HavingNotEnds(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Not().HavingEnds(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrHavingEnds(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Or().HavingEnds(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrHavingNotEnds(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Or().Not().HavingEnds(column, value, caseSensitive, escapeCharacter);
        }

        public Query HavingContains(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return AddComponent(new BasicStringCondition
            {
                Engine = EngineScope,
                Component = "having",
                Operator = "contains",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query HavingNotContains(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Not().HavingContains(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrHavingContains(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Or().HavingContains(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrHavingNotContains(string column, object value, bool caseSensitive = false,
            string escapeCharacter = null)
        {
            return Or().Not().HavingContains(column, value, caseSensitive, escapeCharacter);
        }

        public Query HavingBetween<T>(string column, T lower, T higher)
        {
            return AddComponent(new BetweenCondition<T>
            {
                Engine = EngineScope,
                Component = "having",
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
            // If the developer has passed a string they most likely want a List<string>
            // since string is considered as List<char>
            if (values is string val)
            {
                return AddComponent(new InCondition<string>
                {
                    Engine = EngineScope,
                    Component = "having",
                    Column = column,
                    IsOr = GetOr(),
                    IsNot = GetNot(),
                    Values = ImmutableArray.Create(val)
                });
            }

            return AddComponent(new InCondition<T>
            {
                Engine = EngineScope,
                Component = "having",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Values = values.Distinct().ToImmutableArray()
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
            return AddComponent(new InQueryCondition
            {
                Engine = EngineScope,
                Component = "having",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Query = query
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
        ///     Perform a sub query Having clause
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
            return AddComponent(new QueryCondition
            {
                Engine = EngineScope,
                Component = "having",
                Column = column,
                Operator = op,
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
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
                throw new ArgumentException(
                    $"{nameof(FromClause)} cannot be empty if used inside a {nameof(HavingExists)} condition");

            // simplify the query as much as possible
            query = query.Clone().RemoveComponent("select")
                .SelectRaw("1")
                .Limit(1);

            return AddComponent(new ExistsCondition
            {
                Engine = EngineScope,
                Component = "having",
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
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
            return AddComponent(new BasicDateCondition
            {
                Engine = EngineScope,
                Component = "having",
                Operator = op,
                Column = column,
                Value = value,
                Part = part,
                IsOr = GetOr(),
                IsNot = GetNot()
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
