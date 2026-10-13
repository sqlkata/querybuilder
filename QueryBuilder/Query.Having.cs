using System;
using System.Collections.Generic;

namespace SqlKata
{
    public partial class Query
    {
        public Query Having(string column, string op, object value)
        {
            Havings.Where(column, op, value);
            return this;
        }

        public Query HavingNot(string column, string op, object value)
        {
            Havings.WhereNot(column, op, value);
            return this;
        }

        public Query OrHaving(string column, string op, object value)
        {
            Havings.OrWhere(column, op, value);
            return this;
        }

        public Query OrHavingNot(string column, string op, object value)
        {
            Havings.OrWhereNot(column, op, value);
            return this;
        }

        public Query Having(string column, object value) => Having(column, "=", value);
        public Query HavingNot(string column, object value) => HavingNot(column, "=", value);
        public Query OrHaving(string column, object value) => OrHaving(column, "=", value);
        public Query OrHavingNot(string column, object value) => OrHavingNot(column, "=", value);

        public Query Having(object constraints)
        {
            Havings.Where(constraints);
            return this;
        }

        public Query Having(IEnumerable<KeyValuePair<string, object>> values)
        {
            Havings.Where(values);
            return this;
        }

        public Query HavingRaw(string sql, params object[] bindings)
        {
            Havings.WhereRaw(sql, bindings);
            return this;
        }

        public Query OrHavingRaw(string sql, params object[] bindings)
        {
            Havings.OrWhereRaw(sql, bindings);
            return this;
        }

        public Query Having(Func<Query, Query> callback)
        {
            var query = callback.Invoke(NewQuery());

            if (query.Havings.HasConditions(EngineScope))
            {
                Havings.AddComponent("having", new NestedCondition
                {
                    Conditions = query.Havings,
                    IsNot = Havings.GetNot(),
                    IsOr = Havings.GetOr(),
                });
            }

            return this;
        }

        public Query Having(ConditionGroup group)
        {
            if (group.HasConditions(EngineScope))
            {
                Havings.AddComponent("having", new NestedCondition
                {
                    Conditions = group,
                    IsNot = Havings.GetNot(),
                    IsOr = Havings.GetOr(),
                });
            }

            return this;
        }

        public Query HavingNot(Func<Query, Query> callback)
        {
            Havings.Not();
            return Having(callback);
        }

        public Query HavingNot(ConditionGroup group)
        {
            Havings.Not();
            return Having(group);
        }

        public Query OrHaving(Func<Query, Query> callback)
        {
            Havings.Or();
            return Having(callback);
        }

        public Query OrHaving(ConditionGroup group)
        {
            Havings.Or();
            return Having(group);
        }

        public Query OrHavingNot(Func<Query, Query> callback)
        {
            Havings.Or().Not();
            return Having(callback);
        }

        public Query OrHavingNot(ConditionGroup group)
        {
            Havings.Or().Not();
            return Having(group);
        }

        public Query HavingColumns(string first, string op, string second)
        {
            Havings.WhereColumns(first, op, second);
            return this;
        }

        public Query OrHavingColumns(string first, string op, string second)
        {
            Havings.OrWhereColumns(first, op, second);
            return this;
        }

        public Query HavingNull(string column)
        {
            Havings.WhereNull(column);
            return this;
        }

        public Query HavingNotNull(string column)
        {
            Havings.WhereNotNull(column);
            return this;
        }

        public Query OrHavingNull(string column)
        {
            Havings.OrWhereNull(column);
            return this;
        }

        public Query OrHavingNotNull(string column)
        {
            Havings.OrWhereNotNull(column);
            return this;
        }

        public Query HavingTrue(string column)
        {
            Havings.WhereTrue(column);
            return this;
        }

        public Query HavingFalse(string column)
        {
            Havings.WhereFalse(column);
            return this;
        }

        public Query OrHavingTrue(string column)
        {
            Havings.OrWhereTrue(column);
            return this;
        }

        public Query OrHavingFalse(string column)
        {
            Havings.OrWhereFalse(column);
            return this;
        }

        public Query HavingLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.WhereLike(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query HavingNotLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.WhereNotLike(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrHavingLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.OrWhereLike(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrHavingNotLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.OrWhereNotLike(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query HavingStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.WhereStarts(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query HavingNotStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.WhereNotStarts(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrHavingStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.OrWhereStarts(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrHavingNotStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.OrWhereNotStarts(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query HavingEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.WhereEnds(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query HavingNotEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.WhereNotEnds(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrHavingEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.OrWhereEnds(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrHavingNotEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.OrWhereNotEnds(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query HavingContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.WhereContains(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query HavingNotContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.WhereNotContains(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrHavingContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.OrWhereContains(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrHavingNotContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Havings.OrWhereNotContains(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query HavingBetween<T>(string column, T lower, T higher)
        {
            Havings.WhereBetween(column, lower, higher);
            return this;
        }

        public Query HavingNotBetween<T>(string column, T lower, T higher)
        {
            Havings.WhereNotBetween(column, lower, higher);
            return this;
        }

        public Query OrHavingBetween<T>(string column, T lower, T higher)
        {
            Havings.OrWhereBetween(column, lower, higher);
            return this;
        }

        public Query OrHavingNotBetween<T>(string column, T lower, T higher)
        {
            Havings.OrWhereNotBetween(column, lower, higher);
            return this;
        }

        public Query HavingIn<T>(string column, IEnumerable<T> values)
        {
            Havings.WhereIn(column, values);
            return this;
        }

        public Query HavingNotIn<T>(string column, IEnumerable<T> values)
        {
            Havings.WhereNotIn(column, values);
            return this;
        }

        public Query OrHavingIn<T>(string column, IEnumerable<T> values)
        {
            Havings.OrWhereIn(column, values);
            return this;
        }

        public Query OrHavingNotIn<T>(string column, IEnumerable<T> values)
        {
            Havings.OrWhereNotIn(column, values);
            return this;
        }

        public Query HavingIn(string column, Query query)
        {
            Havings.WhereIn(column, query);
            return this;
        }

        public Query HavingIn(string column, Func<Query, Query> callback)
        {
            return HavingIn(column, callback.Invoke(NewQuery()));
        }

        public Query HavingNotIn(string column, Query query)
        {
            Havings.WhereNotIn(column, query);
            return this;
        }

        public Query HavingNotIn(string column, Func<Query, Query> callback)
        {
            return HavingNotIn(column, callback.Invoke(NewQuery()));
        }

        public Query OrHavingIn(string column, Query query)
        {
            Havings.OrWhereIn(column, query);
            return this;
        }

        public Query OrHavingIn(string column, Func<Query, Query> callback)
        {
            return OrHavingIn(column, callback.Invoke(NewQuery()));
        }

        public Query OrHavingNotIn(string column, Query query)
        {
            Havings.OrWhereNotIn(column, query);
            return this;
        }

        public Query OrHavingNotIn(string column, Func<Query, Query> callback)
        {
            return OrHavingNotIn(column, callback.Invoke(NewQuery()));
        }

        public Query Having(string column, string op, Query query)
        {
            Havings.Where(column, op, query);
            return this;
        }

        public Query Having(string column, string op, Func<Query, Query> callback)
        {
            return Having(column, op, callback.Invoke(NewQuery()));
        }

        public Query OrHaving(string column, string op, Query query)
        {
            Havings.OrWhere(column, op, query);
            return this;
        }

        public Query OrHaving(string column, string op, Func<Query, Query> callback)
        {
            return OrHaving(column, op, callback.Invoke(NewQuery()));
        }

        public Query HavingSub(Query query, object value)
        {
            Havings.WhereSub(query, value);
            return this;
        }

        public Query HavingSub(Query query, string op, object value)
        {
            Havings.WhereSub(query, op, value);
            return this;
        }

        public Query OrHavingSub(Query query, object value)
        {
            Havings.OrWhereSub(query, value);
            return this;
        }

        public Query OrHavingSub(Query query, string op, object value)
        {
            Havings.OrWhereSub(query, op, value);
            return this;
        }

        public Query HavingExists(Query query)
        {
            Havings.WhereExists(query);
            return this;
        }

        public Query HavingExists(Func<Query, Query> callback)
        {
            return HavingExists(callback.Invoke(NewQuery()));
        }

        public Query HavingNotExists(Query query)
        {
            Havings.WhereNotExists(query);
            return this;
        }

        public Query HavingNotExists(Func<Query, Query> callback)
        {
            return HavingNotExists(callback.Invoke(NewQuery()));
        }

        public Query OrHavingExists(Query query)
        {
            Havings.OrWhereExists(query);
            return this;
        }

        public Query OrHavingExists(Func<Query, Query> callback)
        {
            return OrHavingExists(callback.Invoke(NewQuery()));
        }

        public Query OrHavingNotExists(Query query)
        {
            Havings.OrWhereNotExists(query);
            return this;
        }

        public Query OrHavingNotExists(Func<Query, Query> callback)
        {
            return OrHavingNotExists(callback.Invoke(NewQuery()));
        }

        #region Date
        public Query HavingDatePart(string part, string column, string op, object value)
        {
            Havings.WhereDatePart(part, column, op, value);
            return this;
        }

        public Query HavingNotDatePart(string part, string column, string op, object value)
        {
            Havings.WhereNotDatePart(part, column, op, value);
            return this;
        }

        public Query OrHavingDatePart(string part, string column, string op, object value)
        {
            Havings.OrWhereDatePart(part, column, op, value);
            return this;
        }

        public Query OrHavingNotDatePart(string part, string column, string op, object value)
        {
            Havings.OrWhereNotDatePart(part, column, op, value);
            return this;
        }

        public Query HavingDate(string column, string op, object value)
        {
            Havings.WhereDate(column, op, value);
            return this;
        }

        public Query HavingNotDate(string column, string op, object value)
        {
            Havings.WhereNotDate(column, op, value);
            return this;
        }

        public Query OrHavingDate(string column, string op, object value)
        {
            Havings.OrWhereDate(column, op, value);
            return this;
        }

        public Query OrHavingNotDate(string column, string op, object value)
        {
            Havings.OrWhereNotDate(column, op, value);
            return this;
        }

        public Query HavingTime(string column, string op, object value)
        {
            Havings.WhereTime(column, op, value);
            return this;
        }

        public Query HavingNotTime(string column, string op, object value)
        {
            Havings.WhereNotTime(column, op, value);
            return this;
        }

        public Query OrHavingTime(string column, string op, object value)
        {
            Havings.OrWhereTime(column, op, value);
            return this;
        }

        public Query OrHavingNotTime(string column, string op, object value)
        {
            Havings.OrWhereNotTime(column, op, value);
            return this;
        }

        public Query HavingDatePart(string part, string column, object value) => HavingDatePart(part, column, "=", value);
        public Query HavingNotDatePart(string part, string column, object value) => HavingNotDatePart(part, column, "=", value);
        public Query OrHavingDatePart(string part, string column, object value) => OrHavingDatePart(part, column, "=", value);
        public Query OrHavingNotDatePart(string part, string column, object value) => OrHavingNotDatePart(part, column, "=", value);

        public Query HavingDate(string column, object value) => HavingDate(column, "=", value);
        public Query HavingNotDate(string column, object value) => HavingNotDate(column, "=", value);
        public Query OrHavingDate(string column, object value) => OrHavingDate(column, "=", value);
        public Query OrHavingNotDate(string column, object value) => OrHavingNotDate(column, "=", value);

        public Query HavingTime(string column, object value) => HavingTime(column, "=", value);
        public Query HavingNotTime(string column, object value) => HavingNotTime(column, "=", value);
        public Query OrHavingTime(string column, object value) => OrHavingTime(column, "=", value);
        public Query OrHavingNotTime(string column, object value) => OrHavingNotTime(column, "=", value);
        #endregion
    }
}
