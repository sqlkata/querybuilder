namespace SqlKata.SqlExpressions
{
    public class Lower : AbstractSqlExpression
    {
        public AbstractSqlExpression Value { get; }

        public Lower(AbstractSqlExpression expression)
        {
            this.Value = expression;
        }

        public Lower(string column)
        {
            this.Value = new Identifier(column);
        }
    }
}