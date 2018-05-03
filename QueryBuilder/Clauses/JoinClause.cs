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

        /// <inheritdoc />
        public override object[] GetBindings(string engine)
        {
            return Join.GetBindings(engine).ToArray();
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BaseJoin
            {
                Engine = Engine,
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

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new DeepJoin
            {
                Engine = Engine,
                Component = Component,
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