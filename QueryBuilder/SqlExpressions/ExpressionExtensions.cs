namespace SqlKata.SqlExpressions
{
    public static class ExpressionExtensions
    {
        public static SqlExpression As(this SqlExpression source, string alias)
        {
            return new SelectAlias(source, alias);
        }
    }
}