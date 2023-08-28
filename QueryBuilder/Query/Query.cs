using System.Collections.Immutable;

namespace SqlKata
{
    public partial class Query
    {
        public Query()
        {
        }

        public Query(string table, string? comment = null)
        {
            From(table);
            Comment(comment);
        }

        public string GetComment()
        {
            return _comment ?? "";
        }

        public bool HasOffset(string? engineCode = null)
        {
            return GetOffset(engineCode) > 0;
        }

        public bool HasLimit(string? engineCode = null)
        {
            return GetLimit(engineCode) > 0;
        }

        internal long GetOffset(string? engineCode = null)
        {
            engineCode ??= EngineScope;
            var offset = GetOneComponent<OffsetClause>("offset", engineCode);

            return offset?.Offset ?? 0;
        }

        internal int GetLimit(string? engineCode = null)
        {
            engineCode ??= EngineScope;
            var limit = GetOneComponent<LimitClause>("limit", engineCode);

            return limit?.Limit ?? 0;
        }

        public virtual Query Clone()
        {
            return new Query
            {
                _comment = _comment,
                Components = Components.Clone(),
                Parent = Parent,
                QueryAlias = QueryAlias,
                IsDistinct = IsDistinct,
                Method = Method,
                Includes = Includes,
                Variables = Variables
            };
        }

        public Query As(string alias)
        {
            QueryAlias = alias;
            return this;
        }

        /// <summary>
        ///     Sets a comment for the query.
        /// </summary>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        public Query Comment(string? comment)
        {
            _comment = comment;
            return this;
        }

        public Query For(string engine, Func<Query, Query> fn)
        {
            EngineScope = engine;

            var result = fn.Invoke(this);

            // reset the engine
            EngineScope = null;

            return result;
        }

        public Query With(Query query)
        {
            // Clear query alias and add it to the containing clause
            if (string.IsNullOrWhiteSpace(query.QueryAlias))
                throw new InvalidOperationException("No Alias found for the CTE query");

            query = query.Clone();

            var alias = query.QueryAlias?.Trim();

            // clear the query alias
            query.QueryAlias = null;

            return AddComponent(new QueryFromClause
            {
                Engine = EngineScope,
                Component = "cte",
                Query = query,
                Alias = alias
            });
        }

        public Query With(Func<Query, Query> fn)
        {
            return With(fn.Invoke(new Query()));
        }

        public Query With(string alias, Query query)
        {
            return With(query.As(alias));
        }

        public Query With(string alias, Func<Query, Query> fn)
        {
            return With(alias, fn.Invoke(new Query()));
        }

        /// <summary>
        ///     Constructs an ad-hoc table of the given data as a CTE.
        /// </summary>
        public Query With(string alias, IEnumerable<string> columns, IEnumerable<IEnumerable<object?>> valuesCollection)
        {
            ArgumentNullException.ThrowIfNull(alias);
            ArgumentNullException.ThrowIfNull(columns);
            ArgumentNullException.ThrowIfNull(valuesCollection);
            var col = columns is ImmutableArray<string> l ? l : columns.ToImmutableArray();
            var values = valuesCollection is IReadOnlyList<ICollection<object?>> r
                ? r
                : valuesCollection.Select(v => v.ToList()).ToList();

            if (col.Length == 0 || values.Count == 0)
                throw new InvalidOperationException("Columns and valuesCollection cannot be null or empty");

            if (values.Any(row => row.Count != col.Length))
                throw new InvalidOperationException("Columns count should be equal to each Values count");

            return AddComponent(new AdHocTableFromClause
            {
                Engine = EngineScope,
                Component = "cte",
                Alias = alias,
                Columns = col,
                Values = values.SelectMany(x => x).ToImmutableArray()
            });
        }

        public Query WithRaw(string alias, string sql, params object[] bindings)
        {
            ArgumentNullException.ThrowIfNull(alias);
            ArgumentNullException.ThrowIfNull(sql);
            ArgumentNullException.ThrowIfNull(bindings);
            return AddComponent(new RawFromClause
            {
                Engine = EngineScope,
                Component = "cte",
                Alias = alias,
                Expression = sql,
                Bindings = bindings.ToImmutableArray()
            });
        }

        public Query Limit(int value)
        {
            return AddOrReplaceComponent(new LimitClause
            {
                Engine = EngineScope,
                Component = "limit",
                Limit = value < 0 ? 0 : value
            });
        }

        public Query Offset(long value)
        {
            return AddOrReplaceComponent(new OffsetClause
            {
                Engine = EngineScope,
                Component = "offset",
                Offset = value < 0 ? 0 : value
            });
        }

        public Query Offset(int value)
        {
            return Offset((long)value);
        }

        /// <summary>
        ///     Alias for Limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Query Take(int limit)
        {
            return Limit(limit);
        }

        /// <summary>
        ///     Alias for Offset
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Query Skip(int offset)
        {
            return Offset(offset);
        }

        /// <summary>
        ///     Set the limit and offset for a given page.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public Query ForPage(int page, int perPage = 15)
        {
            return Skip((page - 1) * perPage).Take(perPage);
        }

        public Query Distinct()
        {
            IsDistinct = true;
            return this;
        }

        /// <summary>
        ///     Apply the callback's query changes if the given "condition" is true.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="whenTrue">Invoked when the condition is true</param>
        /// <param name="whenFalse">Optional, invoked when the condition is false</param>
        /// <returns></returns>
        public Query When(bool condition, Func<Query, Query>? whenTrue, Func<Query, Query>? whenFalse = null)
        {
            if (condition && whenTrue != null) return whenTrue.Invoke(this);

            if (!condition && whenFalse != null) return whenFalse.Invoke(this);

            return this;
        }

        /// <summary>
        ///     Apply the callback's query changes if the given "condition" is false.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Query WhenNot(bool condition, Func<Query, Query> callback)
        {
            if (!condition) return callback.Invoke(this);

            return this;
        }

        public Query OrderBy(params string[] columns)
        {
            foreach (var column in columns)
                AddComponent(new OrderBy
                {
                    Engine = EngineScope,
                    Component = "order",
                    Column = column,
                    Ascending = true
                });

            return this;
        }

        public Query OrderByDesc(params string[] columns)
        {
            foreach (var column in columns)
                AddComponent(new OrderBy
                {
                    Engine = EngineScope,
                    Component = "order",
                    Column = column,
                    Ascending = false
                });

            return this;
        }

        public Query OrderByRaw(string expression, params object[] bindings)
        {
            return AddComponent(new RawOrderBy
            {
                Engine = EngineScope,
                Component = "order",
                Expression = expression,
                Bindings = bindings.FlattenOneLevel().ToImmutableArray()
            });
        }

        public Query OrderByRandom(string seed)
        {
            return AddComponent(new OrderByRandom()
            {
                Engine = EngineScope,
                Component = "order",
            });
        }

        public Query GroupBy(params string[] columns)
        {
            foreach (var column in columns)
                AddComponent(new Column
                {
                    Engine = EngineScope,
                    Component = "group",
                    Name = column
                });

            return this;
        }

        public Query GroupByRaw(string expression, params object?[] bindings)
        {
            AddComponent(new RawColumn
            {
                Engine = EngineScope,
                Component = "group",
                Expression = expression,
                Bindings = bindings.ToImmutableArray()
            });

            return this;
        }

        public Query Include(string relationName, Query query,
            string? foreignKey = null, string localKey = "Id",
            bool isMany = false)
        {
            Includes.Add(new Include
            {
                Name = relationName,
                LocalKey = localKey,
                ForeignKey = foreignKey,
                Query = query,
                IsMany = isMany
            });

            return this;
        }

        public Query IncludeMany(string relationName, Query query,
            string? foreignKey = null, string localKey = "Id")
        {
            return Include(relationName, query, foreignKey, localKey, true);
        }

        /// <summary>
        ///     Define a variable to be used within the query
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Query Define(string variable, object? value)
        {
            Variables.Add(variable, value);

            return this;
        }

        public object? FindVariable(string variable)
        {
            var found = Variables.ContainsKey(variable);

            if (found) return Variables[variable];

            if (Parent != null) return Parent.FindVariable(variable);

            throw new Exception($"Variable '{variable}' not found");
        }

        /// <summary>
        ///     Gather a list of key-values representing the properties of the object and their values.
        /// </summary>
        /// <param name="data">The plain C# object</param>
        /// <param name="considerKeys">
        ///     When true it will search for properties with the [Key] attribute
        ///     and will add it automatically to the Where clause
        /// </param>
        /// <returns></returns>
        private IReadOnlyDictionary<string, object?> BuildKeyValuePairsFromObject(object data,
            bool considerKeys = false)
        {
            var dynamicData = data.ToDynamicData();
            if (considerKeys)
                foreach (var (name, value) in dynamicData.Keys)
                    Where(name, value);
            return dynamicData.Properties;
        }
    }
}
