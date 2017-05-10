using System;
using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractJoin : AbstractClause
    {

    }

    public class BaseJoin : AbstractJoin
    {
        public Join Join { get; set; }
        public override object[] Bindings
        {
            get
            {
                return Join.Bindings.ToArray();
            }
        }

        public override AbstractClause Clone()
        {
            return new BaseJoin
            {
                Join = Join.Clone(),
                Component = Component,
            };
        }
    }

    public class DeepJoin : AbstractJoin
    {
        public string Type { get; set; }
        public string Expression { get; set; }
        public string SourceKeySuffix { get; set; }
        public string TargetKey { get; set; }
        public Func<string, string> SourceKeyGenerator { get; set; }
        public Func<string, string> TargetKeyGenerator { get; set; }
        public override AbstractClause Clone()
        {
            return new DeepJoin
            {
                Type = Type,
                Expression = Expression,
                SourceKeySuffix = SourceKeySuffix,
                TargetKey = TargetKey,
                SourceKeyGenerator = SourceKeyGenerator,
                TargetKeyGenerator = TargetKeyGenerator,
            };
        }
    }

}