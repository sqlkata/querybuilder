using System;
using System.Collections.Generic;

namespace SqlKata
{
    public partial class Query
    {
        public Query Where(string column, string op, object value)
        {
            Wheres.Where(column, op, value);
            return this;
        }

        public Query WhereNot(string column, string op, object value)
        {
            Wheres.WhereNot(column, op, value);
            return this;
        }

        public Query OrWhere(string column, string op, object value)
        {
            Wheres.OrWhere(column, op, value);
            return this;
        }

        public Query OrWhereNot(string column, string op, object value)
        {
            Wheres.OrWhereNot(column, op, value);
            return this;
        }

        public Query Where(string column, object value) => Where(column, "=", value);
        public Query WhereNot(string column, object value) => WhereNot(column, "=", value);
        public Query OrWhere(string column, object value) => OrWhere(column, "=", value);
        public Query OrWhereNot(string column, object value) => OrWhereNot(column, "=", value);

        public Query Where(object constraints)
        {
            Wheres.Where(constraints);
            return this;
        }

        public Query Where(IEnumerable<KeyValuePair<string, object>> values)
        {
            Wheres.Where(values);
            return this;
        }

        public Query WhereRaw(string sql, params object[] bindings)
        {
            Wheres.WhereRaw(sql, bindings);
            return this;
        }

        public Query OrWhereRaw(string sql, params object[] bindings)
        {
            Wheres.OrWhereRaw(sql, bindings);
            return this;
        }

        public Query Where(Func<Query, Query> callback)
        {
            var query = callback.Invoke(NewQuery());

            if (query.Wheres.HasConditions(EngineScope))
            {
                Wheres.AddComponent("where", new NestedCondition
                {
                    Conditions = query.Wheres,
                    IsNot = Wheres.GetNot(),
                    IsOr = Wheres.GetOr(),
                });
            }

            return this;
        }

        public Query Where(ConditionGroup group)
        {
            if (group.HasConditions(EngineScope))
            {
                Wheres.AddComponent("where", new NestedCondition
                {
                    Conditions = group,
                    IsNot = Wheres.GetNot(),
                    IsOr = Wheres.GetOr(),
                });
            }

            return this;
        }

        public Query WhereNot(Func<Query, Query> callback)
        {
            Wheres.Not();
            return Where(callback);
        }

        public Query WhereNot(ConditionGroup group)
        {
            Wheres.Not();
            return Where(group);
        }

        public Query OrWhere(Func<Query, Query> callback)
        {
            Wheres.Or();
            return Where(callback);
        }

        public Query OrWhere(ConditionGroup group)
        {
            Wheres.Or();
            return Where(group);
        }

        public Query OrWhereNot(Func<Query, Query> callback)
        {
            Wheres.Or().Not();
            return Where(callback);
        }

        public Query OrWhereNot(ConditionGroup group)
        {
            Wheres.Or().Not();
            return Where(group);
        }

        public Query WhereColumns(string first, string op, string second)
        {
            Wheres.WhereColumns(first, op, second);
            return this;
        }

        public Query OrWhereColumns(string first, string op, string second)
        {
            Wheres.OrWhereColumns(first, op, second);
            return this;
        }

        public Query WhereNull(string column)
        {
            Wheres.WhereNull(column);
            return this;
        }

        public Query WhereNotNull(string column)
        {
            Wheres.WhereNotNull(column);
            return this;
        }

        public Query OrWhereNull(string column)
        {
            Wheres.OrWhereNull(column);
            return this;
        }

        public Query OrWhereNotNull(string column)
        {
            Wheres.OrWhereNotNull(column);
            return this;
        }

        public Query WhereTrue(string column)
        {
            Wheres.WhereTrue(column);
            return this;
        }

        public Query WhereFalse(string column)
        {
            Wheres.WhereFalse(column);
            return this;
        }

        public Query OrWhereTrue(string column)
        {
            Wheres.OrWhereTrue(column);
            return this;
        }

        public Query OrWhereFalse(string column)
        {
            Wheres.OrWhereFalse(column);
            return this;
        }

        public Query WhereLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.WhereLike(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query WhereNotLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.WhereNotLike(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrWhereLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.OrWhereLike(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrWhereNotLike(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.OrWhereNotLike(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query WhereStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.WhereStarts(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query WhereNotStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.WhereNotStarts(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrWhereStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.OrWhereStarts(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrWhereNotStarts(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.OrWhereNotStarts(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query WhereEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.WhereEnds(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query WhereNotEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.WhereNotEnds(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrWhereEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.OrWhereEnds(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrWhereNotEnds(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.OrWhereNotEnds(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query WhereContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.WhereContains(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query WhereNotContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.WhereNotContains(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrWhereContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.OrWhereContains(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query OrWhereNotContains(string column, object value, bool caseSensitive = false, string escapeCharacter = null)
        {
            Wheres.OrWhereNotContains(column, value, caseSensitive, escapeCharacter);
            return this;
        }

        public Query WhereBetween<T>(string column, T lower, T higher)
        {
            Wheres.WhereBetween(column, lower, higher);
            return this;
        }

        public Query WhereNotBetween<T>(string column, T lower, T higher)
        {
            Wheres.WhereNotBetween(column, lower, higher);
            return this;
        }

        public Query OrWhereBetween<T>(string column, T lower, T higher)
        {
            Wheres.OrWhereBetween(column, lower, higher);
            return this;
        }

        public Query OrWhereNotBetween<T>(string column, T lower, T higher)
        {
            Wheres.OrWhereNotBetween(column, lower, higher);
            return this;
        }

        public Query WhereIn<T>(string column, IEnumerable<T> values)
        {
            Wheres.WhereIn(column, values);
            return this;
        }

        public Query WhereNotIn<T>(string column, IEnumerable<T> values)
        {
            Wheres.WhereNotIn(column, values);
            return this;
        }

        public Query OrWhereIn<T>(string column, IEnumerable<T> values)
        {
            Wheres.OrWhereIn(column, values);
            return this;
        }

        public Query OrWhereNotIn<T>(string column, IEnumerable<T> values)
        {
            Wheres.OrWhereNotIn(column, values);
            return this;
        }

        public Query WhereIn(string column, Query query)
        {
            Wheres.WhereIn(column, query);
            return this;
        }

        public Query WhereIn(string column, Func<Query, Query> callback)
        {
            return WhereIn(column, callback.Invoke(NewQuery()));
        }

        public Query WhereNotIn(string column, Query query)
        {
            Wheres.WhereNotIn(column, query);
            return this;
        }

        public Query WhereNotIn(string column, Func<Query, Query> callback)
        {
            return WhereNotIn(column, callback.Invoke(NewQuery()));
        }

        public Query OrWhereIn(string column, Query query)
        {
            Wheres.OrWhereIn(column, query);
            return this;
        }

        public Query OrWhereIn(string column, Func<Query, Query> callback)
        {
            return OrWhereIn(column, callback.Invoke(NewQuery()));
        }

        public Query OrWhereNotIn(string column, Query query)
        {
            Wheres.OrWhereNotIn(column, query);
            return this;
        }

        public Query OrWhereNotIn(string column, Func<Query, Query> callback)
        {
            return OrWhereNotIn(column, callback.Invoke(NewQuery()));
        }

        public Query Where(string column, string op, Query query)
        {
            Wheres.Where(column, op, query);
            return this;
        }

        public Query Where(string column, string op, Func<Query, Query> callback)
        {
            return Where(column, op, callback.Invoke(NewQuery()));
        }

        public Query OrWhere(string column, string op, Query query)
        {
            Wheres.OrWhere(column, op, query);
            return this;
        }

        public Query OrWhere(string column, string op, Func<Query, Query> callback)
        {
            return OrWhere(column, op, callback.Invoke(NewQuery()));
        }

        public Query WhereSub(Query query, object value)
        {
            Wheres.WhereSub(query, value);
            return this;
        }

        public Query WhereSub(Query query, string op, object value)
        {
            Wheres.WhereSub(query, op, value);
            return this;
        }

        public Query OrWhereSub(Query query, object value)
        {
            Wheres.OrWhereSub(query, value);
            return this;
        }

        public Query OrWhereSub(Query query, string op, object value)
        {
            Wheres.OrWhereSub(query, op, value);
            return this;
        }

        public Query WhereExists(Query query)
        {
            Wheres.WhereExists(query);
            return this;
        }

        public Query WhereExists(Func<Query, Query> callback)
        {
            return WhereExists(callback.Invoke(NewQuery()));
        }

        public Query WhereNotExists(Query query)
        {
            Wheres.WhereNotExists(query);
            return this;
        }

        public Query WhereNotExists(Func<Query, Query> callback)
        {
            return WhereNotExists(callback.Invoke(NewQuery()));
        }

        public Query OrWhereExists(Query query)
        {
            Wheres.OrWhereExists(query);
            return this;
        }

        public Query OrWhereExists(Func<Query, Query> callback)
        {
            return OrWhereExists(callback.Invoke(NewQuery()));
        }

        public Query OrWhereNotExists(Query query)
        {
            Wheres.OrWhereNotExists(query);
            return this;
        }

        public Query OrWhereNotExists(Func<Query, Query> callback)
        {
            return OrWhereNotExists(callback.Invoke(NewQuery()));
        }

        #region Date
        public Query WhereDatePart(string part, string column, string op, object value)
        {
            Wheres.WhereDatePart(part, column, op, value);
            return this;
        }

        public Query WhereNotDatePart(string part, string column, string op, object value)
        {
            Wheres.WhereNotDatePart(part, column, op, value);
            return this;
        }

        public Query OrWhereDatePart(string part, string column, string op, object value)
        {
            Wheres.OrWhereDatePart(part, column, op, value);
            return this;
        }

        public Query OrWhereNotDatePart(string part, string column, string op, object value)
        {
            Wheres.OrWhereNotDatePart(part, column, op, value);
            return this;
        }

        public Query WhereDate(string column, string op, object value)
        {
            Wheres.WhereDate(column, op, value);
            return this;
        }

        public Query WhereNotDate(string column, string op, object value)
        {
            Wheres.WhereNotDate(column, op, value);
            return this;
        }

        public Query OrWhereDate(string column, string op, object value)
        {
            Wheres.OrWhereDate(column, op, value);
            return this;
        }

        public Query OrWhereNotDate(string column, string op, object value)
        {
            Wheres.OrWhereNotDate(column, op, value);
            return this;
        }

        public Query WhereTime(string column, string op, object value)
        {
            Wheres.WhereTime(column, op, value);
            return this;
        }

        public Query WhereNotTime(string column, string op, object value)
        {
            Wheres.WhereNotTime(column, op, value);
            return this;
        }

        public Query OrWhereTime(string column, string op, object value)
        {
            Wheres.OrWhereTime(column, op, value);
            return this;
        }

        public Query OrWhereNotTime(string column, string op, object value)
        {
            Wheres.OrWhereNotTime(column, op, value);
            return this;
        }

        public Query WhereDatePart(string part, string column, object value) => WhereDatePart(part, column, "=", value);
        public Query WhereNotDatePart(string part, string column, object value) => WhereNotDatePart(part, column, "=", value);
        public Query OrWhereDatePart(string part, string column, object value) => OrWhereDatePart(part, column, "=", value);
        public Query OrWhereNotDatePart(string part, string column, object value) => OrWhereNotDatePart(part, column, "=", value);

        public Query WhereDate(string column, object value) => WhereDate(column, "=", value);
        public Query WhereNotDate(string column, object value) => WhereNotDate(column, "=", value);
        public Query OrWhereDate(string column, object value) => OrWhereDate(column, "=", value);
        public Query OrWhereNotDate(string column, object value) => OrWhereNotDate(column, "=", value);

        public Query WhereTime(string column, object value) => WhereTime(column, "=", value);
        public Query WhereNotTime(string column, object value) => WhereNotTime(column, "=", value);
        public Query OrWhereTime(string column, object value) => OrWhereTime(column, "=", value);
        public Query OrWhereNotTime(string column, object value) => OrWhereNotTime(column, "=", value);
        #endregion
    }
}
