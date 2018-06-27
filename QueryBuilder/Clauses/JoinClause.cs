using System;

namespace SqlKata
{
    public abstract class AbstractJoin : AbstractClause
    {
    }

    public class BaseJoin : AbstractJoin
    {
        #region Properties
        public Join Join { get; internal set; }
        #endregion

        #region Clone
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BaseJoin
            {
                Engine = Engine,
                Join = Join.Clone(),
                Component = Component
            };
        }
        #endregion
    }

    public class DeepJoin : AbstractJoin
    {
        #region Properties
        public string Type { get; internal set; }

        public string Expression { get; internal set; }

        public string SourceKeySuffix { get; internal set; }

        public string TargetKey { get; internal set; }

        public Func<string, string> SourceKeyGenerator { get; internal set; }

        public Func<string, string> TargetKeyGenerator { get; internal set; }
        #endregion

        #region Clone
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
                TargetKeyGenerator = TargetKeyGenerator
            };
        }
        #endregion
    }
}