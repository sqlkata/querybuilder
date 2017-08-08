namespace SqlKata
{
    public class InsertClause : AbstractClause
    {
        public string Column { get; set; }
        public object Value { get; set; }

        public override object[] GetBindings(string engine)
        {
            return new object[] { Value };
        }

        public override AbstractClause Clone()
        {
            return new InsertClause
            {
                Engine = Engine,
                Component = Component,
                Column = Column,
                Value = Value,
            };
        }
    }

}