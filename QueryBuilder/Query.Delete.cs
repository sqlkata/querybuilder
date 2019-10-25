namespace SqlKata
{
    public partial class Query
    {
        public Query AsDelete()
        {
            Method = QueryMethod.Delete;
            return this;
        }

    }
}