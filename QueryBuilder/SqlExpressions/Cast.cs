using System.Collections.Generic;

namespace SqlKata.SqlExpressions
{
    public enum CastType
    {
        Varchar,
        Decimal,
        Float,
        Integer,
        Bool,
        Date,
        DateTime,
        Time,

    }

    public class Cast : SqlExpression, HasBinding
    {
        public SqlExpression Value { get; }
        public CastType TargetType { get; }

        public Cast(SqlExpression expression, CastType targetType)
        {
            this.Value = expression;
            this.TargetType = targetType;
        }

        public Cast(string column, CastType targetType)
        {
            this.Value = new Identifier(column);
            this.TargetType = targetType;
        }

        public IEnumerable<object> GetBindings()
        {
            return Value is HasBinding hasBinding ? hasBinding.GetBindings() : null;
        }
    }
}