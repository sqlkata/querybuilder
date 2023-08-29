using System.Collections.Immutable;
using System.Reflection;

namespace SqlKata
{
    public partial class Query
    {
        public Query Where(string column, string op, object? value)
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

            return AddComponent(new BasicCondition
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                Operator = op,
                Value = value,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query WhereNot(string column, string op, object? value)
        {
            return Not().Where(column, op, value);
        }

        public Query OrWhere(string column, string op, object? value)
        {
            return Or().Where(column, op, value);
        }

        public Query OrWhereNot(string column, string op, object? value)
        {
            return Or().Not().Where(column, op, value);
        }

        public Query Where(string column, object? value)
        {
            return Where(column, "=", value);
        }

        public Query WhereNot(string column, object? value)
        {
            return WhereNot(column, "=", value);
        }

        public Query OrWhere(string column, object? value)
        {
            return OrWhere(column, "=", value);
        }

        public Query OrWhereNot(string column, object? value)
        {
            return OrWhereNot(column, "=", value);
        }

        /// <summary>
        ///     Perform a where constraint
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public Query Where(object constraints)
        {
            var dictionary = new Dictionary<string, object?>();

            foreach (var item in constraints.GetType().GetRuntimeProperties())
                dictionary.Add(item.Name, item.GetValue(constraints));

            return Where(dictionary);
        }

        public Query Where(IEnumerable<KeyValuePair<string, object?>> values)
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

                query = Not(notFlag).Where(tuple.Key, tuple.Value);
            }

            return query;
        }

        public Query WhereRaw(string sql, params object[] bindings)
        {
            return AddComponent(new RawCondition
            {
                Engine = EngineScope,
                Component = "where",
                Expression = sql,
                Bindings = bindings,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query OrWhereRaw(string sql, params object[] bindings)
        {
            return Or().WhereRaw(sql, bindings);
        }

        /// <summary>
        ///     Apply a nested where clause
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Query Where(Func<Query, Query> callback)
        {
            var query = callback.Invoke(NewChild());

            // omit empty queries
            if (!query.Components.Any("where")) return this;
            
            return AddComponent(new NestedCondition
            {
                Engine = EngineScope,
                Component = "where",
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public Query WhereNot(Func<Query, Query> callback)
        {
            return Not().Where(callback);
        }

        public Query OrWhere(Func<Query, Query> callback)
        {
            return Or().Where(callback);
        }

        public Query OrWhereNot(Func<Query, Query> callback)
        {
            return Not().Or().Where(callback);
        }

        public Query WhereColumns(string first, string op, string second)
        {
            return AddComponent(new TwoColumnsCondition
            {
                Engine = EngineScope,
                Component = "where",
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query OrWhereColumns(string first, string op, string second)
        {
            return Or().WhereColumns(first, op, second);
        }

        public Query WhereNull(string column)
        {
            return AddComponent(new NullCondition
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query WhereNotNull(string column)
        {
            return Not().WhereNull(column);
        }

        public Query OrWhereNull(string column)
        {
            return Or().WhereNull(column);
        }

        public Query OrWhereNotNull(string column)
        {
            return Or().Not().WhereNull(column);
        }

        public Query WhereTrue(string column)
        {
            return AddComponent(new BooleanCondition
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                Value = true,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query OrWhereTrue(string column)
        {
            return Or().WhereTrue(column);
        }

        public Query WhereFalse(string column)
        {
            return AddComponent(new BooleanCondition
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                Value = false,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query OrWhereFalse(string column)
        {
            return Or().WhereFalse(column);
        }

        public Query WhereLike(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return AddComponent(new BasicStringCondition
            {
                Engine = EngineScope,
                Component = "where",
                Operator = "like",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query WhereNotLike(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return Not().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrWhereLike(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return Or().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrWhereNotLike(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return Or().Not().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public Query WhereStarts(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return AddComponent(new BasicStringCondition
            {
                Engine = EngineScope,
                Component = "where",
                Operator = "starts",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query WhereNotStarts(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return Not().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrWhereStarts(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return Or().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrWhereNotStarts(string column, object value, bool caseSensitive = false,
            char? escapeCharacter = null)
        {
            return Or().Not().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public Query WhereEnds(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return AddComponent(new BasicStringCondition
            {
                Engine = EngineScope,
                Component = "where",
                Operator = "ends",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query WhereNotEnds(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return Not().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrWhereEnds(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return Or().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrWhereNotEnds(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return Or().Not().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public Query WhereContains(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return AddComponent(new BasicStringCondition
            {
                Engine = EngineScope,
                Component = "where",
                Operator = "contains",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query WhereNotContains(string column, object value, bool caseSensitive = false,
            char? escapeCharacter = null)
        {
            return Not().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrWhereContains(string column, object value, bool caseSensitive = false, char? escapeCharacter = null)
        {
            return Or().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public Query OrWhereNotContains(string column, object value, bool caseSensitive = false,
            char? escapeCharacter = null)
        {
            return Or().Not().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public Query WhereBetween<T>(string column, T lower, T higher)
            where T: notnull
        {
            return AddComponent(new BetweenCondition
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Lower = lower,
                Higher = higher
            });
        }

        public Query OrWhereBetween<T>(string column, T lower, T higher)
            where T : notnull
        {
            return Or().WhereBetween(column, lower, higher);
        }

        public Query WhereNotBetween<T>(string column, T lower, T higher)
            where T : notnull
        {
            return Not().WhereBetween(column, lower, higher);
        }

        public Query OrWhereNotBetween<T>(string column, T lower, T higher)
            where T : notnull
        {
            return Or().Not().WhereBetween(column, lower, higher);
        }

        public Query WhereIn<T>(string column, params T[] values) =>
            WhereIn(column, (IEnumerable<T>)values);
        public Query WhereIn<T>(string column, IEnumerable<T> values)
        {
            // If the developer has passed a string they most likely want a List<string>
            // since string is considered as List<char>
            if (values is string val)
            {
                return AddComponent(new InCondition
                {
                    Engine = EngineScope,
                    Component = "where",
                    Column = column,
                    IsOr = GetOr(),
                    IsNot = GetNot(),
                    Values = ImmutableArray.Create<object>(val)
                });
            }

            return AddComponent(new InCondition
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Values = values.Distinct().Cast<object>().ToImmutableArray()
            });
        }

        public Query OrWhereIn<T>(string column, IEnumerable<T> values)
        {
            return Or().WhereIn(column, values);
        }

        public Query WhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Not().WhereIn(column, values);
        }

        public Query OrWhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Or().Not().WhereIn(column, values);
        }


        public Query WhereIn(string column, Query query)
        {
            return AddComponent(new InQueryCondition
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Query = query
            });
        }

        public Query WhereIn(string column, Func<Query, Query> callback)
        {
            var query = callback.Invoke(new Query().SetParent(this));

            return WhereIn(column, query);
        }

        public Query OrWhereIn(string column, Query query)
        {
            return Or().WhereIn(column, query);
        }

        public Query OrWhereIn(string column, Func<Query, Query> callback)
        {
            return Or().WhereIn(column, callback);
        }

        public Query WhereNotIn(string column, Query query)
        {
            return Not().WhereIn(column, query);
        }

        public Query WhereNotIn(string column, Func<Query, Query> callback)
        {
            return Not().WhereIn(column, callback);
        }

        public Query OrWhereNotIn(string column, Query query)
        {
            return Or().Not().WhereIn(column, query);
        }

        public Query OrWhereNotIn(string column, Func<Query, Query> callback)
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
        public Query Where(string column, string op, Func<Query, Query> callback)
        {
            var query = callback.Invoke(NewChild());

            return Where(column, op, query);
        }

        public Query Where(string column, string op, Query query)
        {
            return AddComponent(new QueryCondition
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                Operator = op,
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public Query WhereSub(Query query, object value)
        {
            return WhereSub(query, "=", value);
        }

        public Query WhereSub(Query query, string op, object value)
        {
            return AddComponent(new SubQueryCondition
            {
                Engine = EngineScope,
                Component = "where",
                Value = value,
                Operator = op,
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public Query OrWhereSub(Query query, object value)
        {
            return Or().WhereSub(query, value);
        }

        public Query OrWhereSub(Query query, string op, object value)
        {
            return Or().WhereSub(query, op, value);
        }

        public Query OrWhere(string column, string op, Query query)
        {
            return Or().Where(column, op, query);
        }

        public Query OrWhere(string column, string op, Func<Query, Query> callback)
        {
            return Or().Where(column, op, callback);
        }

        public Query WhereExists(Query query)
        {
            if (!query.HasComponent("from"))
                throw new ArgumentException(
                    $"'{nameof(FromClause)}' cannot be empty if used inside a '{nameof(WhereExists)}' condition");

            return AddComponent(new ExistsCondition
            {
                Engine = EngineScope,
                Component = "where",
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public Query WhereExists(Func<Query, Query> callback)
        {
            var childQuery = new Query().SetParent(this);
            return WhereExists(callback.Invoke(childQuery));
        }

        public Query WhereNotExists(Query query)
        {
            return Not().WhereExists(query);
        }

        public Query WhereNotExists(Func<Query, Query> callback)
        {
            return Not().WhereExists(callback);
        }

        public Query OrWhereExists(Query query)
        {
            return Or().WhereExists(query);
        }

        public Query OrWhereExists(Func<Query, Query> callback)
        {
            return Or().WhereExists(callback);
        }

        public Query OrWhereNotExists(Query query)
        {
            return Or().Not().WhereExists(query);
        }

        public Query OrWhereNotExists(Func<Query, Query> callback)
        {
            return Or().Not().WhereExists(callback);
        }

        #region date

        public Query WhereDatePart(string part, string column, string op, object value)
        {
            return AddComponent(new BasicDateCondition
            {
                Engine = EngineScope,
                Component = "where",
                Operator = op,
                Column = column,
                Value = value,
                Part = part.ToLowerInvariant(),
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public Query WhereNotDatePart(string part, string column, string op, object value)
        {
            return Not().WhereDatePart(part, column, op, value);
        }

        public Query OrWhereDatePart(string part, string column, string op, object value)
        {
            return Or().WhereDatePart(part, column, op, value);
        }

        public Query OrWhereNotDatePart(string part, string column, string op, object value)
        {
            return Or().Not().WhereDatePart(part, column, op, value);
        }

        public Query WhereDate(string column, string op, object value)
        {
            return WhereDatePart("date", column, op, value);
        }

        public Query WhereNotDate(string column, string op, object value)
        {
            return Not().WhereDate(column, op, value);
        }

        public Query OrWhereDate(string column, string op, object value)
        {
            return Or().WhereDate(column, op, value);
        }

        public Query OrWhereNotDate(string column, string op, object value)
        {
            return Or().Not().WhereDate(column, op, value);
        }

        public Query WhereTime(string column, string op, object value)
        {
            return WhereDatePart("time", column, op, value);
        }

        public Query WhereNotTime(string column, string op, object value)
        {
            return Not().WhereTime(column, op, value);
        }

        public Query OrWhereTime(string column, string op, object value)
        {
            return Or().WhereTime(column, op, value);
        }

        public Query OrWhereNotTime(string column, string op, object value)
        {
            return Or().Not().WhereTime(column, op, value);
        }

        public Query WhereDatePart(string part, string column, object value)
        {
            return WhereDatePart(part, column, "=", value);
        }

        public Query WhereNotDatePart(string part, string column, object value)
        {
            return WhereNotDatePart(part, column, "=", value);
        }

        public Query OrWhereDatePart(string part, string column, object value)
        {
            return OrWhereDatePart(part, column, "=", value);
        }

        public Query OrWhereNotDatePart(string part, string column, object value)
        {
            return OrWhereNotDatePart(part, column, "=", value);
        }

        public Query WhereDate(string column, object value)
        {
            return WhereDate(column, "=", value);
        }

        public Query WhereNotDate(string column, object value)
        {
            return WhereNotDate(column, "=", value);
        }

        public Query OrWhereDate(string column, object value)
        {
            return OrWhereDate(column, "=", value);
        }

        public Query OrWhereNotDate(string column, object value)
        {
            return OrWhereNotDate(column, "=", value);
        }

        public Query WhereTime(string column, object value)
        {
            return WhereTime(column, "=", value);
        }

        public Query WhereNotTime(string column, object value)
        {
            return WhereNotTime(column, "=", value);
        }

        public Query OrWhereTime(string column, object value)
        {
            return OrWhereTime(column, "=", value);
        }

        public Query OrWhereNotTime(string column, object value)
        {
            return OrWhereNotTime(column, "=", value);
        }

        #endregion
    }
}
