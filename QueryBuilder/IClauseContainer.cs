using System.Collections.Generic;

namespace SqlKata
{
    public interface IClauseContainer
    {
        bool HasComponent(string component, string engineCode = null);
        List<C> GetComponents<C>(string component, string engineCode = null) where C : AbstractClause;
        IClauseContainer Clone();
    }
}
