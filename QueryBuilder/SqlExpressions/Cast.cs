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

    public class Cast : AbstractSqlExpression
    {
        public AbstractSqlExpression Value { get; }
        public CastType TargetType { get; }

        public Cast(AbstractSqlExpression expression, CastType targetType)
        {
            this.Value = expression;
            this.TargetType = targetType;
        }

        public Cast(string column, CastType targetType)
        {
            this.Value = new Identifier(column);
            this.TargetType = targetType;
        }
    }
}