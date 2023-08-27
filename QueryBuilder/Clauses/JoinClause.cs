namespace SqlKata
{
    public abstract class AbstractJoin : AbstractClause
    {
    }

    public class BaseJoin : AbstractJoin
    {
        public Join Join { get; set; }
    }

    public class DeepJoin : AbstractJoin
    {
        public string Type { get; set; }
        public string Expression { get; set; }
        public string SourceKeySuffix { get; set; }
        public string TargetKey { get; set; }
        public Func<string, string> SourceKeyGenerator { get; set; }
        public Func<string, string> TargetKeyGenerator { get; set; }
    }
}
