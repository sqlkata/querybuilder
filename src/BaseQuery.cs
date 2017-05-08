using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers;

namespace SqlKata
{
    public abstract class AbstractQuery
    {
        protected AbstractQuery Parent;
    }
    public abstract partial class BaseQuery<Q> : AbstractQuery where Q : BaseQuery<Q>
    {
        protected Compiler _compiler;
        protected string[] bindingsOrder = new[] {
            "select", "from", "join", "where", "group", "having", "order", "limit", "union",
        };
        public List<AbstractClause> Clauses { get; set; } = new List<AbstractClause>();

        private bool orFlag = false;
        private bool notFlag = false;


        public virtual List<object> Bindings
        {
            get
            {
                var hasBindings = bindingsOrder
                    .Select(x => Get(x))
                    .Where(x => x.Any() && x.Where(b => b.Bindings.Count() > 0).Any())
                    .ToList();

                var bindings = hasBindings
                .SelectMany(x => x)
                .Select(x => x.Bindings)
                .ToList()
                .Where(x => x != null)
                .SelectMany(x => x)
                .ToList();

                return bindings;
            }
        }

        public BaseQuery(Compiler compiler)
        {
            _compiler = compiler;
        }

        public Q SetParent(AbstractQuery parent)
        {
            if (this == parent)
            {
                throw new ArgumentException("Cannot set the same query as a parent of itself");
            }

            this.Parent = (Q)parent;
            return (Q)this;
        }

        public abstract Q NewQuery();

        public Q NewChild()
        {
            return NewQuery().SetParent((Q)this);
        }

        /// <summary>
        /// Add a component clause to the query.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="clause"></param>
        /// <returns></returns>
        public Q Add(string component, AbstractClause clause)
        {
            clause.Component = component;
            Clauses.Add(clause);

            return (Q)this;
        }

        /// <summary>
        /// Get the list of clauses for a component.
        /// </summary>
        /// <returns></returns>
        public List<C> Get<C>(string component) where C : AbstractClause
        {
            return Clauses.Where(x => x.Component == component).Cast<C>().ToList();
        }

        /// <summary>
        /// Get the list of clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public List<AbstractClause> Get(string component)
        {
            return Get<AbstractClause>(component);
        }

        /// <summary>
        /// Get a single component clause from the query.
        /// </summary>
        /// <returns></returns>
        public C GetOne<C>(string component) where C : AbstractClause
        {
            return Get<C>(component).FirstOrDefault();
        }

        /// <summary>
        /// Get a single component clause from the query.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public AbstractClause GetOne(string component)
        {
            return GetOne<AbstractClause>(component);
        }

        /// <summary>
        /// Return wether the query has clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public bool Has(string component)
        {
            return Get(component).Any();
        }

        /// <summary>
        /// Remove all clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public Q Clear(string component)
        {
            Clauses = Clauses.Where(x => x.Component != component).ToList();
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
        protected Q Or()
        {
            orFlag = true;
            return (Q)this;
        }

        /// <summary>
        /// Set the next "not" operator for the "where" clause.
        /// </summary>
        /// <returns></returns>        
        protected Q Not(bool flag)
        {
            notFlag = flag;
            return (Q)this;
        }

        /// <summary>
        /// Get the boolean operator and reset it to "and"
        /// </summary>
        /// <returns></returns>
        protected bool getOr()
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
        protected bool getNot()
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
            return Clear("from").Add("from", new From
            {
                Table = table
            });
        }

        public Q From(Query query, string alias = null)
        {
            query.SetParent((Q)this);

            if (alias != null)
            {
                query.As(alias);
            };

            return Clear("from").Add("from", new QueryFrom
            {
                Query = query
            });
        }

        public Q FromRaw(string expression, params object[] bindings)
        {
            return Clear("from").Add("from", new RawFrom
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });
        }

        public Q From(Raw expression)
        {
            return FromRaw(expression.Value, expression.Bindings);
        }

        public Q From(Func<Query, Query> callback, string alias = null)
        {
            var query = new Query(_compiler);

            query.SetParent((Q)this);

            return From(callback.Invoke(query), alias);
        }

    }
}