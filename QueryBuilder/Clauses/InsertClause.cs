using System;
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

        internal ConfigurationInsert ConfigurationInsert { get; set; } = null;
        internal string PrimaryKeyName { get; set; } = "";
        internal Type PrimaryKeyType { get; set; } = null;

        public override object[] GetBindings(string engine)
        {
            return Values.ToArray();
        }

        public override AbstractClause Clone()
        {
            return new InsertClause
            {
                Engine = Engine,
                Component = Component,
                Columns = Columns,
                Values = Values,
                PrimaryKeyName = PrimaryKeyName, 
                PrimaryKeyType = PrimaryKeyType
            };
        }
    }

    public class InsertQueryClause : AbstractInsertClause
    {
        public List<string> Columns { get; set; }
        public Query Query { get; set; }

        public override object[] GetBindings(string engine)
        {
            return Query.GetBindings(engine).ToArray();
        }

        public override AbstractClause Clone()
        {
            return new InsertQueryClause
            {
                Engine = Engine,
                Component = Component,
                Columns = Columns,
                Query = Query.Clone()
            };
        }
    }

}