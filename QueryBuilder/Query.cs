using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata
{
    public partial class Query
    {
        private ConditionGroup _wheres;
        public ConditionGroup Wheres
        {
            get => _wheres;
            set
            {
                _wheres = value;
                if (_wheres != null)
                {
                    _wheres.OnGetOr = () => Havings?.SetOr(false);
                    _wheres.OnGetNot = () => Havings?.SetNot(false);
                }
            }
        }

        private ConditionGroup _havings;
        public ConditionGroup Havings
        {
            get => _havings;
            set
            {
                _havings = value;
                if (_havings != null)
                {
                    _havings.OnGetOr = () => Wheres?.SetOr(false);
                    _havings.OnGetNot = () => Wheres?.SetNot(false);
                }
            }
        }

        public List<BaseJoin> Joins { get; set; } = [];
        public List<AbstractFrom> FromClauses { get; set; } = [];
        public List<AbstractColumn> SelectClauses { get; set; } = [];
        public List<AbstractFrom> CteClauses { get; set; } = [];
        public List<AbstractOrderBy> OrderByClauses { get; set; } = [];
        public List<AbstractColumn> GroupByClauses { get; set; } = [];
        public List<LimitClause> LimitClauses { get; set; } = [];
        public List<OffsetClause> OffsetClauses { get; set; } = [];
        public List<AggregateClause> AggregateClauses { get; set; } = [];
        public List<AbstractCombine> CombineClauses { get; set; } = [];
        public List<AbstractInsertClause> InsertClauses { get; set; } = [];
        public List<AbstractClause> UpdateClauses { get; set; } = [];

        private string _engineScope = null;
        public string EngineScope
        {
            get => _engineScope;
            set
            {
                _engineScope = value;
                if (Wheres != null) Wheres.EngineScope = value;
                if (Havings != null) Havings.EngineScope = value;
            }
        }

        public Query SetEngineScope(string engine)
        {
            this.EngineScope = engine;
            return this;
        }


        private string comment;

        public bool IsDistinct { get; set; } = false;
        public string QueryAlias { get; set; }
        public string Method { get; set; } = "select";
        public List<Include> Includes = [];
        public Dictionary<string, object> Variables = [];

        public Query()
        {
            Wheres = new ConditionGroup();
            Havings = new ConditionGroup();
        }

        public Query(string table, string comment = null) : this()
        {
            From(table);
            Comment(comment);
        }

        /// <summary>
        /// Copy all clauses, conditions, and joins from the source query.
        /// </summary>
        public Query CopyClausesFrom(Query source)
        {
            this.Wheres = new ConditionGroup { Clauses = source.Wheres.Clauses.Select(x => x.Clone()).ToList() };
            this.Havings = new ConditionGroup { Clauses = source.Havings.Clauses.Select(x => x.Clone()).ToList() };
            this.Joins = source.Joins.Select(x => (BaseJoin)x.Clone()).ToList();
            this.FromClauses = source.FromClauses.Select(x => (AbstractFrom)x.Clone()).ToList();
            this.SelectClauses = source.SelectClauses.Select(x => (AbstractColumn)x.Clone()).ToList();
            this.CteClauses = source.CteClauses.Select(x => (AbstractFrom)x.Clone()).ToList();
            this.OrderByClauses = source.OrderByClauses.Select(x => (AbstractOrderBy)x.Clone()).ToList();
            this.GroupByClauses = source.GroupByClauses.Select(x => (AbstractColumn)x.Clone()).ToList();
            this.LimitClauses = source.LimitClauses.Select(x => (LimitClause)x.Clone()).ToList();
            this.OffsetClauses = source.OffsetClauses.Select(x => (OffsetClause)x.Clone()).ToList();
            this.AggregateClauses = source.AggregateClauses.Select(x => (AggregateClause)x.Clone()).ToList();
            this.CombineClauses = source.CombineClauses.Select(x => (AbstractCombine)x.Clone()).ToList();
            this.InsertClauses = source.InsertClauses.Select(x => (AbstractInsertClause)x.Clone()).ToList();
            this.UpdateClauses = source.UpdateClauses.Select(x => x.Clone()).ToList();
            return this;
        }

        public virtual Query Clone()
        {
            var clone = new Query();
            clone.CopyClausesFrom(this);
            clone.EngineScope = EngineScope;
            clone.QueryAlias = QueryAlias;
            clone.IsDistinct = IsDistinct;
            clone.Method = Method;
            clone.Includes = Includes.Select(i => i.Clone()).ToList();
            clone.Variables = new Dictionary<string, object>(Variables);
            return clone;
        }

        internal Query AddOrReplace<T>(List<T> list, T clause, string engineCode = null) where T : AbstractClause
        {
            engineCode ??= EngineScope;
            clause.Engine = engineCode;
            var current = list.SingleOrDefault(c => c.Engine == engineCode);
            if (current != null)
                list.Remove(current);
            list.Add(clause);
            return this;
        }

        public Query Or()
        {
            Wheres.Or();
            Havings.Or();
            return this;
        }

        public Query Not(bool flag = true)
        {
            Wheres.Not(flag);
            Havings.Not(flag);
            return this;
        }

        public string GetComment() => comment ?? "";

        public bool HasOffset(string engineCode = null) => GetOffset(engineCode) > 0;

        public bool HasLimit(string engineCode = null) => GetLimit(engineCode) > 0;

        internal long GetOffset(string engineCode = null)
        {
            engineCode ??= EngineScope;
            var offset = OffsetClauses.ForEngineOrDefault(engineCode);

            return offset?.Offset ?? 0;
        }

        internal int GetLimit(string engineCode = null)
        {
            engineCode ??= EngineScope;
            var limit = LimitClauses.ForEngineOrDefault(engineCode);

            return limit?.Limit ?? 0;
        }

        public Query As(string alias)
        {
            QueryAlias = alias;
            return this;
        }

        /// <summary>
        /// Sets a comment for the query.
        /// </summary>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        public Query Comment(string comment)
        {
            this.comment = comment;
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
            {
                throw new InvalidOperationException("No Alias found for the CTE query");
            }

            query = query.Clone();

            var alias = query.QueryAlias.Trim();

            // clear the query alias
            query.QueryAlias = null;

            CteClauses.Add(new QueryFromClause
            {
                Query = query,
                Alias = alias,
                Engine = EngineScope
            });
            return this;
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
        /// Constructs an ad-hoc table of the given data as a CTE.
        /// </summary>
        public Query With(string alias, IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection)
        {
            var columnsList = columns?.ToList();
            var valuesCollectionList = valuesCollection?.ToList();

            if ((columnsList?.Count ?? 0) == 0 || (valuesCollectionList?.Count ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and valuesCollection cannot be null or empty");
            }

            var clause = new AdHocTableFromClause()
            {
                Alias = alias,
                Columns = columnsList,
                Values = new List<object>(),
                Engine = EngineScope
            };

            foreach (var values in valuesCollectionList)
            {
                var valuesList = values.ToList();
                if (columnsList.Count != valuesList.Count)
                {
                    throw new InvalidOperationException("Columns count should be equal to each Values count");
                }

                clause.Values.AddRange(valuesList);
            }

            CteClauses.Add(clause);
            return this;
        }

        public Query WithRaw(string alias, string sql, params object[] bindings)
        {
            CteClauses.Add(new RawFromClause
            {
                Alias = alias,
                Expression = sql,
                Bindings = bindings,
                Engine = EngineScope
            });
            return this;
        }

        public Query Limit(int value)
        {
            return AddOrReplace(LimitClauses, new LimitClause
            {
                Limit = value
            });
        }

        public Query Offset(long value)
        {
            return AddOrReplace(OffsetClauses, new OffsetClause
            {
                Offset = value
            });
        }

        public Query Offset(int value)
        {
            return Offset((long)value);
        }

        /// <summary>
        /// Alias for Limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Query Take(int limit)
        {
            return Limit(limit);
        }

        /// <summary>
        /// Alias for Offset
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Query Skip(int offset)
        {
            return Offset(offset);
        }

        /// <summary>
        /// Set the limit and offset for a given page.
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
        /// Apply the callback's query changes if the given "condition" is true.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="whenTrue">Invoked when the condition is true</param>
        /// <param name="whenFalse">Optional, invoked when the condition is false</param>
        /// <returns></returns>
        public Query When(bool condition, Func<Query, Query> whenTrue, Func<Query, Query> whenFalse = null)
        {
            if (condition && whenTrue != null)
            {
                return whenTrue.Invoke(this);
            }

            if (!condition && whenFalse != null)
            {
                return whenFalse.Invoke(this);
            }

            return this;
        }

        /// <summary>
        /// Apply the callback's query changes if the given "condition" is false.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Query WhenNot(bool condition, Func<Query, Query> callback)
        {
            if (!condition)
            {
                return callback.Invoke(this);
            }

            return this;
        }

        public Query OrderBy(params string[] columns)
        {
            foreach (var column in columns)
            {
                OrderByClauses.Add(new OrderBy
                {
                    Column = column,
                    Ascending = true,
                    Engine = EngineScope
                });
            }

            return this;
        }

        public Query OrderByDesc(params string[] columns)
        {
            foreach (var column in columns)
            {
                OrderByClauses.Add(new OrderBy
                {
                    Column = column,
                    Ascending = false,
                    Engine = EngineScope
                });
            }

            return this;
        }

        public Query OrderByRaw(string expression, params object[] bindings)
        {
            OrderByClauses.Add(new RawOrderBy
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray(),
                Engine = EngineScope
            });
            return this;
        }

        public Query OrderByRandom(string seed)
        {
            OrderByClauses.Add(new OrderByRandom
            {
                Engine = EngineScope
            });
            return this;
        }

        public Query GroupBy(params string[] columns)
        {
            foreach (var column in columns)
            {
                GroupByClauses.Add(new Column
                {
                    Name = column,
                    Engine = EngineScope
                });
            }

            return this;
        }

        public Query GroupByRaw(string expression, params object[] bindings)
        {
            GroupByClauses.Add(new RawColumn
            {
                Expression = expression,
                Bindings = bindings,
                Engine = EngineScope
            });

            return this;
        }

        public virtual Query NewQuery()
        {
            return new Query();
        }

        public Query Include(string relationName, Query query, string foreignKey = null, string localKey = "Id", bool isMany = false)
        {

            Includes.Add(new Include
            {
                Name = relationName,
                LocalKey = localKey,
                ForeignKey = foreignKey,
                Query = query,
                IsMany = isMany,
            });

            return this;
        }

        public Query IncludeMany(string relationName, Query query, string foreignKey = null, string localKey = "Id")
        {
            return Include(relationName, query, foreignKey, localKey, isMany: true);
        }

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> CacheDictionaryProperties = new ConcurrentDictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Define a variable to be used within the query
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Query Define(string variable, object value)
        {
            Variables.Add(variable, value);

            return this;
        }

        public object FindVariable(string variable)
        {
            var found = Variables.ContainsKey(variable);

            if (found)
            {
                return Variables[variable];
            }

            throw new Exception($"Variable '{variable}' not found");
        }

        /// <summary>
        /// Gather a list of key-values representing the properties of the object and their values.
        /// </summary>
        /// <param name="data">The plain C# object</param>
        /// <param name="considerKeys">
        /// When true it will search for properties with the [Key] attribute
        /// and will add it automatically to the Where clause
        /// </param>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<string, object>> BuildKeyValuePairsFromObject(object data, bool considerKeys = false)
        {
            var dictionary = new Dictionary<string, object>();
            var props = CacheDictionaryProperties.GetOrAdd(data.GetType(), type => type.GetRuntimeProperties().ToArray());

            foreach (var property in props)
            {
                if (property.GetCustomAttribute(typeof(IgnoreAttribute)) != null)
                {
                    continue;
                }

                var value = property.GetValue(data);

                var colAttr = property.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;

                var name = colAttr?.Name ?? property.Name;

                dictionary.Add(name, value);

                if (considerKeys && colAttr != null)
                {
                    if ((colAttr as KeyAttribute) != null)
                    {
                        this.Where(name, value);
                    }
                }
            }

            return dictionary;
        }
    }
}
