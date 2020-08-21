namespace SqlKata.SqlExpressions
{
    public class Count : AbstractSqlExpression
    {
        public AbstractSqlExpression Value { get; }

        public Count(string column)
        {
            Value = new Identifier(column);
        }

        public Count(AbstractSqlExpression expression)
        {
            Value = expression;
        }

    }
}