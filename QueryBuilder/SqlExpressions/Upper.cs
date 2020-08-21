namespace SqlKata.SqlExpressions
{
    public class Upper : AbstractSqlExpression
    {
        public AbstractSqlExpression Value { get; }

        public Upper(AbstractSqlExpression expression)
        {
            this.Value = expression;
        }

        public Upper(string column)
        {
            this.Value = new Identifier(column);
        }
    }
}