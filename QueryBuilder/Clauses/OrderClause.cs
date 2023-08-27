namespace SqlKata
{
    public abstract class AbstractOrderBy : AbstractClause
    {
    }

    public class OrderBy : AbstractOrderBy
    {
        public string Column { get; set; }
        public bool Ascending { get; set; } = true;
    }

    public class RawOrderBy : AbstractOrderBy
    {
        public string Expression { get; set; }
        public object[] Bindings { set; get; }
    }

    public class OrderByRandom : AbstractOrderBy
    {
    }
}
