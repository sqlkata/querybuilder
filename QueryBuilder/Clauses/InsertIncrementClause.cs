using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractInsertIncrementClause : AbstractClause
    {

    }

    public class InsertIncrementClause : AbstractInsertIncrementClause
    {
        public List<string> Columns { get; set; }
        public List<object> Values { get; set; }

        public override object[] GetBindings(string engine)
        {
            return Values.ToArray();
        }

        public override AbstractClause Clone()
        {
            return new InsertIncrementClause
            {
                Engine = Engine,
                Component = Component,
                Columns = Columns,
                Values = Values,
            };
        }
    }
}
