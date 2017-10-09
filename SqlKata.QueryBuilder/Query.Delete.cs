namespace SqlKata.QueryBuilder
{
    public partial class Query
    {
        public Query Delete()
        {
            Method = "delete";
            return this;
        }

    }
}
