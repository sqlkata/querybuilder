namespace SqlKata.SqlExpressions
{
    public class BooleanExpression : SqlExpression
    {
        public SqlExpression Left { get; set; }
        public SqlExpression Right { get; set; }

        public BooleanExpression(SqlExpression left, SqlExpression right)
        {
            Left = left;
            Right = right;
        }
    }
}