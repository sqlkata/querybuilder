using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractInsertClause : AbstractClause
    {

    }

    public class InsertClause : AbstractInsertClause
    {
        public List<string> Columns { get; set; }
        public List<object> Values { get; set; }
        public bool ReturnId { get; set; } = false;

        public override AbstractClause Clone()
        {
            return new InsertClause
            {
                Engine = Engine,
                Component = Component,
                Columns = Columns,
                Values = Values,
                ReturnId = ReturnId,
            };
        }
    }

    public class InsertQueryClause : AbstractInsertClause
    {
        public List<string> Columns { get; set; }
        public Query Query { get; set; }

        public override AbstractClause Clone()
        {
            return new InsertQueryClause
            {
                Engine = Engine,
                Component = Component,
                Columns = Columns,
                Query = Query.Clone(),
            };
        }
    }
}