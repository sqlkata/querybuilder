namespace SqlKata
{
    public interface IComponentList
    {
        List<TC> GetComponents<TC>(string component, string? engineCode = null) where TC : AbstractClause;
        List<AbstractClause> GetComponents(string component, string? engineCode = null);
        TC? GetOneComponent<TC>(string component, string? engineCode = null) where TC : AbstractClause;
        AbstractClause? GetOneComponent(string component, string? engineCode = null);
        bool HasComponent(string component, string? engineCode = null);
        bool Any(string type);
    }

    public sealed class ComponentList : IComponentList
    {
        public ComponentList()
        {
            Clauses = new List<AbstractClause>();
        }

        private ComponentList(List<AbstractClause> clauses)
        {
            Clauses = clauses;
        }

        public List<AbstractClause> Clauses { get; private set; }

        public void AddComponent(AbstractClause clause)
        {
            ArgumentNullException.ThrowIfNull(clause);
            Clauses.Add(clause);
        }

        public void AddOrReplaceComponent(AbstractClause clause)
        {
            ArgumentNullException.ThrowIfNull(clause);
            var countRemoved = Clauses.RemoveAll(
                c => c.Component == clause.Component &&
                     c.Engine == clause.Engine);
            if (countRemoved > 1) throw
                new InvalidOperationException("AddOrReplaceComponent cannot replace a component when there is more than one component to replace!");

            AddComponent(clause);
        }

        public List<TC> GetComponents<TC>(string component, string? engineCode = null) where TC : AbstractClause
        {
            return Clauses
                .Where(x => x.Component == component)
                .Where(x => engineCode == null || x.Engine == null || engineCode == x.Engine)
                .Cast<TC>()
                .ToList();
        }

        public List<AbstractClause> GetComponents(string component, string? engineCode = null)
        {
            return GetComponents<AbstractClause>(component, engineCode);
        }

        public TC? GetOneComponent<TC>(string component, string? engineCode = null) where TC : AbstractClause
        {
            var all = GetComponents<TC>(component, engineCode);
            return all.FirstOrDefault(c => c.Engine == engineCode) ??
                   all.FirstOrDefault(c => c.Engine == null);
        }

        public AbstractClause? GetOneComponent(string component, string? engineCode = null)
        {
            return GetOneComponent<AbstractClause>(component, engineCode);
        }

        public bool HasComponent(string component, string? engineCode = null)
        {
            return GetComponents(component, engineCode).Any();
        }

        public void RemoveComponent(string component, string? engineCode)
        {
            Clauses = Clauses
                .Where(x => !(x.Component == component &&
                              (engineCode == null || x.Engine == null || engineCode == x.Engine)))
                .ToList();
        }

        public ComponentList Clone()
        {
            return new ComponentList(Clauses.ToList());
        }

        public bool Any(string type)
        {
            return Clauses.Any(x => x.Component == type);
        }
    }

    public static class EnumerableExt
    {
        public static List<T>? NullIfEmpty<T>(this List<T> src)
        {
            return src.Count > 0 ? src : default;
        }
    }
}
