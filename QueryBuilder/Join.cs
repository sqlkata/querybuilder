using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public class Join : IConditional
    {
        protected string _type = "inner join";
        private bool orFlag = false;
        private bool notFlag = false;
        protected List<AbstractClause> _clauses = new List<AbstractClause>();

        public ConditionGroup Conditions { get; set; } = new ConditionGroup();
        public AbstractFrom FromClause { get; set; }

        public string EngineScope { get; set; } = null;

        [Obsolete("Use dedicated properties (Conditions, FromClause) instead.")]
        public List<AbstractClause> Clauses
        {
            get
            {
                var all = new List<AbstractClause>(_clauses);
                if (FromClause != null)
                {
                    all.Add(FromClause);
                }
                all.AddRange(Conditions.Clauses);
                return all;
            }
            set
            {
                _clauses.Clear();
                Conditions.Clauses.Clear();
                FromClause = null;
                if (value != null)
                {
                    foreach (var clause in value)
                    {
                        if (clause is AbstractFrom from)
                        {
                            FromClause = from;
                        }
                        else if (clause is AbstractCondition)
                        {
                            Conditions.Clauses.Add(clause);
                        }
                        else
                        {
                            _clauses.Add(clause);
                        }
                    }
                }
            }
        }

        public string Type
        {
            get => _type;
            set => _type = value?.ToUpperInvariant();
        }

        public Join()
        {
        }

        public virtual Join Clone()
        {
            var clone = new Join
            {
                _type = _type,
                EngineScope = EngineScope,
                FromClause = FromClause?.Clone() as AbstractFrom,
                Conditions = Conditions.Clone(),
                _clauses = _clauses.Select(x => x.Clone()).ToList()
            };
            return clone;
        }

        IClauseContainer IClauseContainer.Clone() => Clone();

        public Join AsType(string type)
        {
            Type = type;
            return this;
        }

        public Join AsInner() => AsType("inner join");
        public Join AsOuter() => AsType("outer join");
        public Join AsLeft() => AsType("left join");
        public Join AsRight() => AsType("right join");
        public Join AsCross() => AsType("cross join");

        public Join JoinWith(string table)
        {
            FromClause = new FromClause { Table = table, Engine = EngineScope, Component = "from" };
            return this;
        }

        public Join JoinWith(Query query, string alias = null)
        {
            query = query.Clone();
            if (alias != null)
            {
                query.As(alias);
            }
            FromClause = new QueryFromClause { Query = query, Engine = EngineScope, Component = "from" };
            return this;
        }

        public Join JoinWith(Func<Query, Query> callback, string alias = null)
        {
            return JoinWith(callback.Invoke(new Query()), alias);
        }

        public Join JoinWithRaw(string sql, params object[] bindings)
        {
            FromClause = new RawFromClause { Expression = sql, Bindings = bindings, Engine = EngineScope, Component = "from" };
            return this;
        }

        /// <summary>
        /// Alias for "JoinWith" operator.
        /// </summary>
        public Join From(string table) => JoinWith(table);
        public Join From(Query query, string alias = null) => JoinWith(query, alias);
        public Join From(Func<Query, Query> callback, string alias = null) => JoinWith(callback, alias);
        public Join FromRaw(string sql, params object[] bindings) => JoinWithRaw(sql, bindings);

        [Obsolete("Use dedicated properties (Conditions, FromClause) instead.")]
        public Join AddComponent(string component, AbstractClause clause, string engineCode = null)
        {
            engineCode ??= EngineScope;
            clause.Engine = engineCode;
            clause.Component = component;

            if (clause is AbstractFrom from)
            {
                FromClause = from;
            }
            else if (clause is AbstractCondition)
            {
                Conditions.AddComponent(component, clause, engineCode);
            }
            else
            {
                _clauses.Add(clause);
            }

            return this;
        }

        [Obsolete("Use dedicated properties (Conditions, FromClause) instead.")]
        public Join AddOrReplaceComponent(string component, AbstractClause clause, string engineCode = null)
        {
            return AddComponent(component, clause, engineCode);
        }

        [Obsolete("Use dedicated properties (Conditions, FromClause) instead.")]
        public List<C> GetComponents<C>(string component, string engineCode = null) where C : AbstractClause
        {
            engineCode ??= EngineScope;

            if (component == "from" && FromClause is C cFrom && (engineCode == null || FromClause.Engine == null || FromClause.Engine == engineCode))
            {
                return new List<C> { cFrom };
            }

            if (component == "where")
            {
                return Conditions.GetComponents<C>(null, engineCode);
            }

            return _clauses
                .Where(x => x.Component == component)
                .Where(x => engineCode == null || x.Engine == null || engineCode == x.Engine)
                .Cast<C>()
                .ToList();
        }

        [Obsolete("Use dedicated properties (Conditions, FromClause) instead.")]
        public List<AbstractClause> GetComponents(string component, string engineCode = null)
        {
            return GetComponents<AbstractClause>(component, engineCode);
        }

        [Obsolete("Use dedicated properties (Conditions, FromClause) instead.")]
        public C GetOneComponent<C>(string component, string engineCode = null) where C : AbstractClause
        {
            engineCode ??= EngineScope;
            var all = GetComponents<C>(component, engineCode);
            return all.FirstOrDefault(c => c.Engine == engineCode) ?? all.FirstOrDefault(c => c.Engine == null);
        }

        [Obsolete("Use dedicated properties (Conditions, FromClause) instead.")]
        public AbstractClause GetOneComponent(string component, string engineCode = null)
        {
            return GetOneComponent<AbstractClause>(component, engineCode);
        }

        [Obsolete("Use dedicated properties (Conditions, FromClause) instead.")]
        public bool HasComponent(string component, string engineCode = null)
        {
            engineCode ??= EngineScope;
            if (component == "from") return FromClause != null && (engineCode == null || FromClause.Engine == null || FromClause.Engine == engineCode);
            if (component == "where") return Conditions.HasConditions(engineCode);
            return _clauses.Any(x => x.Component == component && (engineCode == null || x.Engine == null || engineCode == x.Engine));
        }

        [Obsolete("Use dedicated properties (Conditions, FromClause) instead.")]
        public Join ClearComponent(string component, string engineCode = null)
        {
            engineCode ??= EngineScope;

            if (component == "from")
            {
                if (engineCode == null || FromClause?.Engine == engineCode)
                {
                    FromClause = null;
                }
            }
            else if (component == "where")
            {
                Conditions.Clear(engineCode);
            }
            else
            {
                _clauses.RemoveAll(x => x.Component == component && (engineCode == null || x.Engine == null || engineCode == x.Engine));
            }

            return this;
        }

        public Join Or()
        {
            orFlag = true;
            return this;
        }

        public Join Not(bool flag = true)
        {
            notFlag = flag;
            return this;
        }

        public bool GetOr()
        {
            var ret = orFlag;
            orFlag = false;
            return ret;
        }

        public bool GetNot()
        {
            var ret = notFlag;
            notFlag = false;
            return ret;
        }

        public void SetOr(bool value) => orFlag = value;
        public void SetNot(bool value) => notFlag = value;

        void IConditional.AddComponent(string component, AbstractClause clause, string engineCode)
        {
            Conditions.AddComponent("where", clause, engineCode);
        }

        IConditional IConditional.NewChild() => new Join { EngineScope = this.EngineScope };

        public Join On(string first, string second, string op = "=") => this.WhereColumns(first, op, second);
        public Join OrOn(string first, string second, string op = "=") => this.Or().WhereColumns(first, op, second);
    }
}
