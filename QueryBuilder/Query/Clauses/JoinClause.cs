namespace SqlKata
{
    public abstract class AbstractJoin : AbstractClause
    {
    }

    public sealed class BaseJoin : AbstractJoin
    {
        public required Join Join { get; init; }
    }
}
