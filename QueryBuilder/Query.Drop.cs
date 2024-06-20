namespace SqlKata
{
    public partial class Query
    {
        public Query DropTable()
        {
            Method = "Drop";
            return this;
        }
        public Query Truncate()
        {
            Method = "Truncate";
            return this;
        }

    }
}
