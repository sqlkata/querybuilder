using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public abstract class AbstractQuery
    {
        protected AbstractQuery Parent;
    }

    public abstract partial class BaseQuery<Q> : AbstractQuery where Q : BaseQuery<Q>
    {
        public List<AbstractClause> Clauses { get; set; } = new List<AbstractClause>();

        private bool orFlag = false;
        private bool notFlag = false;
        public string EngineScope = null;

        public Q SetEngineScope(string engine)
        {
            this.EngineScope = engine;

            return (Q)this;
        }

        public BaseQuery()
        {
        }

        /// <summary>
        /// Return a cloned copy of the current query.
        /// </summary>
        /// <returns></returns>
        public virtual Q Clone()
        {
            var q = NewQuery();

            q.Clauses = this.Clauses.Select(x => x.Clone()).ToList();

            return q;
        }

        public Q SetParent(AbstractQuery parent)
        {
            if (this == parent)
            {
                throw new ArgumentException("Cannot set the same query as a parent of itself");
            }

            this.Parent = parent;
            return (Q)this;
        }

        public abstract Q NewQuery();

        public Q NewChild()
        {
            var newQuery = NewQuery().SetParent((Q)this);
            newQuery.EngineScope = this.EngineScope;
            return newQuery;
        }

        /// <summary>
        /// Add a component clause to the query.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="clause"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public Q AddComponent(string component, AbstractClause clause, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            clause.Engine = engineCode;
            clause.Component = component;
            Clauses.Add(clause);

            return (Q)this;
        }

        /// <summary>
        /// Get the list of clauses for a component.
        /// </summary>
        /// <returns></returns>
        public List<C> GetComponents<C>(string component, string engineCode = null) where C : AbstractClause
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            var clauses = Clauses
                .Where(x => x.Component == component)
                .Where(x => engineCode == null || x.Engine == null || engineCode == x.Engine)
                .Cast<C>();

            return clauses.ToList();
        }

        /// <summary>
        /// Get the list of clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public List<AbstractClause> GetComponents(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetComponents<AbstractClause>(component, engineCode);
        }

        /// <summary>
        /// Get a single component clause from the query.
        /// </summary>
        /// <returns></returns>
        public C GetOneComponent<C>(string component, string engineCode = null) where C : AbstractClause
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetComponents<C>(component, engineCode)
            .FirstOrDefault();
        }

        /// <summary>
        /// Get a single component clause from the query.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public AbstractClause GetOneComponent(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetOneComponent<AbstractClause>(component, engineCode);
        }

        /// <summary>
        /// Return wether the query has clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public bool HasComponent(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetComponents(component, engineCode).Any();
        }

        /// <summary>
        /// Remove all clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public Q ClearComponent(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            Clauses = Clauses
                .Where(x => !(x.Component == component && (engineCode == null || x.Engine == null || engineCode == x.Engine)))
                .ToList();

            return (Q)this;
        }

        /// <summary>
        /// Set the next boolean operator to "and" for the "where" clause.
        /// </summary>
        /// <returns></returns>
        protected Q And()
        {
            orFlag = false;
            return (Q)this;
        }

        /// <summary>
        /// Set the next boolean operator to "or" for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public Q Or()
        {
            orFlag = true;
            return (Q)this;
        }

        /// <summary>
        /// Set the next "not" operator for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public Q Not(bool flag = true)
        {
            notFlag = flag;
            return (Q)this;
        }

        /// <summary>
        /// Get the boolean operator and reset it to "and"
        /// </summary>
        /// <returns></returns>
        protected bool GetOr()
        {
            var ret = orFlag;

            // reset the flag
            orFlag = false;
            return ret;
        }

        /// <summary>
        /// Get the "not" operator and clear it
        /// </summary>
        /// <returns></returns>
        protected bool GetNot()
        {
            var ret = notFlag;

            // reset the flag
            notFlag = false;
            return ret;
        }

        /// <summary>
        /// Add a from Clause
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public Q From(string table)
        {
            return ClearComponent("from").AddComponent("from", new FromClause
            {
                Table = table
            });
        }

        public Q From(Query query, string alias = null)
        {
            query = query.Clone();
            query.SetParent((Q)this);

            if (alias != null)
            {
                query.As(alias);
            };

            return ClearComponent("from").AddComponent("from", new QueryFromClause
            {
                Query = query
            });
        }

        public Q FromRaw(string sql, params object[] bindings)
        {
            return ClearComponent("from").AddComponent("from", new RawFromClause
            {
                Expression = sql,
                Bindings = bindings,
            });
        }

        public Q From(Func<Query, Query> callback, string alias = null)
        {
            var query = new Query();

            query.SetParent((Q)this);

            return From(callback.Invoke(query), alias);
        }

    }
}