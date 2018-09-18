using SqlKata.Interfaces;

namespace SqlKata
{
    public partial class Query
    {
        public IQuery AsDelete()
        {
            Method = "delete";
            return this;
        }

    }
}