using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public class ConditionGroup : IConditional
    {
        private bool orFlag = false;
        private bool notFlag = false;

        public Action OnGetOr { get; set; }
        public Action OnGetNot { get; set; }

        public string EngineScope { get; set; } = null;
        public List<AbstractClause> Clauses { get; set; }

        public ConditionGroup()
        {
            Clauses = [];
        }

        public virtual ConditionGroup Clone()
        {
            return new ConditionGroup
            {
                EngineScope = EngineScope,
                Clauses = Clauses.Select(x => x.Clone()).ToList()
            };
        }

        IClauseContainer IClauseContainer.Clone() => Clone();

        public bool HasConditions(string engineCode = null)
        {
            engineCode ??= EngineScope;
            return Clauses.Any(x => engineCode == null || x.Engine == null || engineCode == x.Engine);
        }

        public List<AbstractCondition> GetConditions(string engineCode = null)
        {
            return GetComponents<AbstractCondition>(null, engineCode);
        }

        public List<C> GetComponents<C>(string component = null, string engineCode = null) where C : AbstractClause
        {
            engineCode ??= EngineScope;

            return Clauses
                .Where(x => engineCode == null || x.Engine == null || engineCode == x.Engine)
                .OfType<C>()
                .ToList();
        }

        public List<AbstractClause> GetComponents(string component = null, string engineCode = null)
        {
            return GetComponents<AbstractClause>(component, engineCode);
        }

        public C GetOneComponent<C>(string component, string engineCode = null) where C : AbstractClause
        {
            engineCode ??= EngineScope;

            var all = GetComponents<C>(component, engineCode);
            return all.FirstOrDefault(c => c.Engine == engineCode) ?? all.FirstOrDefault(c => c.Engine == null);
        }

        public AbstractClause GetOneComponent(string component, string engineCode = null)
        {
            return GetOneComponent<AbstractClause>(component, engineCode);
        }

        public bool HasComponent(string component, string engineCode = null)
        {
            engineCode ??= EngineScope;
            return GetComponents(component, engineCode).Count != 0;
        }

        public ConditionGroup Clear(string engineCode = null)
        {
            engineCode ??= EngineScope;

            if (engineCode == null)
            {
                Clauses.Clear();
            }
            else
            {
                Clauses.RemoveAll(x => x.Engine == null || x.Engine == engineCode);
            }

            return this;
        }

        public bool GetOr()
        {
            var ret = orFlag;
            orFlag = false;
            OnGetOr?.Invoke();
            return ret;
        }

        public bool GetNot()
        {
            var ret = notFlag;
            notFlag = false;
            OnGetNot?.Invoke();
            return ret;
        }

        public void SetOr(bool value)
        {
            orFlag = value;
        }

        public void SetNot(bool value)
        {
            notFlag = value;
        }

        public void AddComponent(string component, AbstractClause clause, string engineCode = null)
        {
            clause.Engine = engineCode ?? EngineScope;
            if (component != null && clause.Component == null)
            {
                clause.Component = component;
            }
            Clauses.Add(clause);
        }

        public IClauseContainer Add(AbstractClause clause, string engineCode = null)
        {
            AddComponent(clause.Component, clause, engineCode);
            return this;
        }

        public IClauseContainer Add(IReadOnlyCollection<AbstractClause> clauses, string engineCode = null)
        {
            foreach (var clause in clauses)
            {
                Add(clause, engineCode);
            }
            return this;
        }

        public IConditional NewChild() => new ConditionGroup { EngineScope = this.EngineScope };
    }
}
