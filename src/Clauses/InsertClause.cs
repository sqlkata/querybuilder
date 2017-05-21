namespace SqlKata
{
    public class InsertClause : AbstractClause
    {
        public string Column { get; set; }
        public object Value { get; set; }

        public override object[] Bindings => new object[] { Value };

        public override AbstractClause Clone()
        {
            return new InsertClause
            {
                Component = Component,
                Column = Column,
                Value = Value,
            };
        }
    }

}