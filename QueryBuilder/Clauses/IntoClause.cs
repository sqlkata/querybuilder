namespace SqlKata.Clauses
{
    public class IntoClause : AbstractClause
    {
        public string TableName { get; set; }

        public override AbstractClause Clone()
        {
            return new IntoClause()
            {
                TableName = TableName,
                Component = Component
            };
        }
    }
}
