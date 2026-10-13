using System.Collections.Generic;

namespace SqlKata
{
    public interface IConditional : IClauseContainer
    {
        string EngineScope { get; }
        List<AbstractClause> Clauses { get; }
        bool GetOr();
        bool GetNot();
        void SetOr(bool value);
        void SetNot(bool value);
        void AddComponent(string component, AbstractClause clause, string engineCode = null);
        IConditional NewChild();
    }
}
