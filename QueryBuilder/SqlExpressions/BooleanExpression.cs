namespace SqlKata.SqlExpressions
{
    public class BooleanExpression : AbstractSqlExpression
    {
        public AbstractSqlExpression Left { get; set; }
        public AbstractSqlExpression Right { get; set; }

        public BooleanExpression(AbstractSqlExpression left, AbstractSqlExpression right)
        {
            Left = left;
            Right = right;
        }
    }
}