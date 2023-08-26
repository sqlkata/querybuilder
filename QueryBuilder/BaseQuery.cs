using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Extensions;

namespace SqlKata
{
    public partial class Query
    {
        public Query Parent;
        public string EngineScope;
        private bool _notFlag;

        private bool _orFlag;

        public List<AbstractClause> Clauses { get; set; } = new List<AbstractClause>();

        public Query SetEngineScope(string engine)
        {
            EngineScope = engine;

            return (Query)this;
        }

      
        public Query SetParent(Query parent)
        {
            if (this == parent)
                throw new ArgumentException($"Cannot set the same {nameof(Query)} as a parent of itself");

            Parent = parent;
            return (Query)this;
        }

        public Query NewChild()
        {
            var newQuery = NewQuery().SetParent((Query)this);
            newQuery.EngineScope = EngineScope;
            return newQuery;
        }

        /// <summary>
        ///     Add a component clause to the query.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="clause"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public Query AddComponent(string component, AbstractClause clause, string engineCode = null)
        {
            if (engineCode == null) engineCode = EngineScope;

            clause.Engine = engineCode;
            clause.Component = component;
            Clauses.Add(clause);

            return (Query)this;
        }

        /// <summary>
        ///     If the query already contains a clause for the given component
        ///     and engine, replace it with the specified clause. Otherwise, just
        ///     add the clause.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="clause"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public Query AddOrReplaceComponent(string component, AbstractClause clause, string engineCode = null)
        {
            engineCode = engineCode ?? EngineScope;

            var current = GetComponents(component).SingleOrDefault(c => c.Engine == engineCode);
            if (current != null)
                Clauses.Remove(current);

            return AddComponent(component, clause, engineCode);
        }


        /// <summary>
        ///     Get the list of clauses for a component.
        /// </summary>
        /// <returns></returns>
        public List<TC> GetComponents<TC>(string component, string engineCode = null) where TC : AbstractClause
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
        public List<AbstractClause> GetComponents(string component, string engineCode = null)
        {
            if (engineCode == null) engineCode = EngineScope;

            return GetComponents<AbstractClause>(component, engineCode);
        }

        /// <summary>
        ///     Get a single component clause from the query.
        /// </summary>
        /// <returns></returns>
        public TC GetOneComponent<TC>(string component, string engineCode = null) where TC : AbstractClause
        {
            engineCode = engineCode ?? EngineScope;

            var all = GetComponents<TC>(component, engineCode);
            return all.FirstOrDefault(c => c.Engine == engineCode) ?? all.FirstOrDefault(c => c.Engine == null);
        }

        /// <summary>
        ///     Get a single component clause from the query.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public AbstractClause GetOneComponent(string component, string engineCode = null)
        {
            if (engineCode == null) engineCode = EngineScope;

            return GetOneComponent<AbstractClause>(component, engineCode);
        }

        /// <summary>
        ///     Return whether the query has clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public bool HasComponent(string component, string engineCode = null)
        {
            if (engineCode == null) engineCode = EngineScope;

            return GetComponents(component, engineCode).Any();
        }

        /// <summary>
        ///     Remove all clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public Query ClearComponent(string component, string engineCode = null)
        {
            if (engineCode == null) engineCode = EngineScope;

            Clauses = Clauses
                .Where(x => !(x.Component == component &&
                              (engineCode == null || x.Engine == null || engineCode == x.Engine)))
                .ToList();

            return (Query)this;
        }

        /// <summary>
        ///     Set the next boolean operator to "and" for the "where" clause.
        /// </summary>
        /// <returns></returns>
        protected Query And()
        {
            _orFlag = false;
            return (Query)this;
        }

        /// <summary>
        ///     Set the next boolean operator to "or" for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public Query Or()
        {
            _orFlag = true;
            return (Query)this;
        }

        /// <summary>
        ///     Set the next "not" operator for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public Query Not(bool flag = true)
        {
            _notFlag = flag;
            return (Query)this;
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
            return AddOrReplaceComponent("from", new FromClause
            {
                Table = table.Name
            });
        }
        public Query From(string table)
        {
            return AddOrReplaceComponent("from", new FromClause
            {
                Table = table
            });
        }

        public Query From(Query query, string alias = null)
        {
            query = query.Clone();
            query.SetParent((Query)this);

            if (alias != null) query.As(alias);
            

            return AddOrReplaceComponent("from", new QueryFromClause
            {
                Query = query
            });
        }

        public Query FromRaw(string sql, params object[] bindings)
        {
            return AddOrReplaceComponent("from", new RawFromClause
            {
                Expression = sql,
                Bindings = bindings
            });
        }

        public Query From(Func<Query, Query> callback, string alias = null)
        {
            var query = new Query();

            query.SetParent((Query)this);

            return From(callback.Invoke(query), alias);
        }
    }
}
