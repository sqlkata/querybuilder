namespace SqlKata
{
    public partial class Query
    {
        public Query AsDelete()
        {
            Method = "delete";
            return this;
        }

        //public Query AsDeleteRaw(string expression, params object[] bindings)
        //{
        //    Method = "delete";

        //    AddComponent("delete", new RawColumn
        //    {
        //        Expression = expression,
        //        Bindings = Helper.Flatten(bindings).ToArray()
        //    });
        //    return this;
        //}
    }
}