namespace SqlKata.Clauses
{
    public class CreateTableAsClause : AbstractClause
    {
        public Query SelectQuery { get; set; }
        public override AbstractClause Clone()
        {
            return new CreateTableAsClause
            {
                SelectQuery = SelectQuery.Clone()
            };
        }
    }
}
