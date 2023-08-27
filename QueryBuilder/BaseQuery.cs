using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using SqlKata.Extensions;

namespace SqlKata
{
    public partial class Query
    {
        public Query? Parent;
        public string? EngineScope;
        private bool _notFlag;

        private bool _orFlag;

        public List<AbstractClause> Clauses { get; set; } = new();

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> CacheDictionaryProperties = new();

        private string? _comment;
        public List<Include> Includes = new();
        public Dictionary<string, object?> Variables = new();
        public bool IsDistinct { get; set; }
        // Mandatory for CTE queries
        public string? QueryAlias { get; set; }
        public string Method { get; set; } = "select";
        public Query SetEngineScope(string? engine)
        {
            EngineScope = engine;

            return this;
        }


        public Query SetParent(Query? parent)
        {
            if (this == parent)
                throw new ArgumentException($"Cannot set the same {nameof(Query)} as a parent of itself");

            Parent = parent;
            return this;
        }

        public Query NewChild()
        {
            var newQuery = new Query().SetParent(this);
            newQuery.EngineScope = EngineScope;
            return newQuery;
        }

        /// <summary>
        ///     Add a component clause to the query.
        /// </summary>
        public Query AddComponent(AbstractClause clause)
        {
            Debug.Assert(clause.Component != null);
            Clauses.Add(clause);

            return this;
        }

        /// <summary>
        ///     If the query already contains a clause for the given component
        ///     and engine, replace it with the specified clause. Otherwise, just
        ///     add the clause.
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public Query AddOrReplaceComponent(AbstractClause clause)
        {
            var countRemoved = Clauses.RemoveAll(
                c => c.Component == clause.Component &&
                     c.Engine == clause.Engine);
            if (countRemoved > 1) throw
                new InvalidOperationException("AddOrReplaceComponent cannot replace a component when there is more than one component to replace!");

            return AddComponent(clause);
        }


        /// <summary>
        ///     Get the list of clauses for a component.
        /// </summary>
        /// <returns></returns>
        public List<TC> GetComponents<TC>(string component, string? engineCode = null) where TC : AbstractClause
        {
            if (engineCode == null) engineCode = EngineScope;

            var clauses = Clauses
                .Where(x => x.Component == component)
                .Where(x => engineCode == null || x.Engine == null || engineCode == x.Engine)
                .Cast<TC>();

            return clauses.ToList();
        }

        /// <summary>
        ///     Get the list of clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public List<AbstractClause> GetComponents(string component, string? engineCode = null)
        {
            engineCode ??= EngineScope;

            return GetComponents<AbstractClause>(component, engineCode);
        }

        /// <summary>
        ///     Get a single component clause from the query.
        /// </summary>
        /// <returns></returns>
        public TC? GetOneComponent<TC>(string component, string? engineCode = null) where TC : AbstractClause
        {
            engineCode ??= EngineScope;

            var all = GetComponents<TC>(component, engineCode);
            return all.FirstOrDefault(c => c.Engine == engineCode) ??
                   all.FirstOrDefault(c => c.Engine == null);
        }

        /// <summary>
        ///     Get a single component clause from the query.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public AbstractClause? GetOneComponent(string component, string? engineCode = null)
        {
            engineCode ??= EngineScope;

            return GetOneComponent<AbstractClause>(component, engineCode);
        }

        /// <summary>
        ///     Return whether the query has clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public bool HasComponent(string component, string? engineCode = null)
        {
            engineCode ??= EngineScope;

            return GetComponents(component, engineCode).Any();
        }

        /// <summary>
        ///     Remove all clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public Query RemoveComponent(string component, string? engineCode = null)
        {
            engineCode ??= EngineScope;

            Clauses = Clauses
                .Where(x => !(x.Component == component &&
                              (engineCode == null || x.Engine == null || engineCode == x.Engine)))
                .ToList();

            return this;
        }

        /// <summary>
        ///     Set the next boolean operator to "and" for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public Query And()
        {
            _orFlag = false;
            return this;
        }

        /// <summary>
        ///     Set the next boolean operator to "or" for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public Query Or()
        {
            _orFlag = true;
            return this;
        }

        /// <summary>
        ///     Set the next "not" operator for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public Query Not(bool flag = true)
        {
            _notFlag = flag;
            return this;
        }

        /// <summary>
        ///     Get the boolean operator and reset it to "and"
        /// </summary>
        /// <returns></returns>
        public bool GetOr()
        {
            var ret = _orFlag;

            // reset the flag
            _orFlag = false;
            return ret;
        }

        /// <summary>
        ///     Get the "not" operator and clear it
        /// </summary>
        /// <returns></returns>
        public bool GetNot()
        {
            var ret = _notFlag;

            // reset the flag
            _notFlag = false;
            return ret;
        }

        /// <summary>
        ///     Add a from Clause
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public Query From(GTable table)
        {
            return AddOrReplaceComponent(new FromClause
            {
                Engine = EngineScope,
                Component = "from",
                Table = table.Name
            });
        }
        public Query From(string table)
        {
            return AddOrReplaceComponent(new FromClause
            {
                Engine = EngineScope,
                Component = "from",
                Table = table
            });
        }

        public Query From(Query query, string? alias = null)
        {
            query = query.Clone();
            query.SetParent(this);

            if (alias != null) query.As(alias);


            return AddOrReplaceComponent(new QueryFromClause
            {
                Engine = EngineScope,
                Component = "from",
                Query = query
            });
        }

        public Query FromRaw(string sql, params object[] bindings)
        {
            ArgumentNullException.ThrowIfNull(sql);
            ArgumentNullException.ThrowIfNull(bindings);
            return AddOrReplaceComponent(new RawFromClause
            {
                Engine = EngineScope,
                Component = "from",
                Expression = sql,
                Bindings = bindings.ToImmutableArray()
            });
        }

        public Query From(Func<Query, Query> callback, string? alias = null)
        {
            var query = new Query();

            query.SetParent(this);

            return From(callback.Invoke(query), alias);
        }
    }
}
