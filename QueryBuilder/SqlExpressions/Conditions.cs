using System.Linq.Expressions;

namespace SqlKata.SqlExpressions
{
    public static class Conditions
    {
        public static Expression Condition(Expression column, string op, Expression value)
        {
            return Expression.Block(
                column,
                new Literal(op),
                value
            );
        }

        public static Expression Condition(string column, string op, object value)
        {
            return Expression.Block(
                new Identifier(column),
                new Literal(op),
                new Literal(value)
            );
        }

        public static Expression GreaterThan(Expression column, Expression value)
        {
            return Condition(column, ">", value);
        }

        public static Expression GreaterThan(string column, object value)
        {
            return Condition(column, ">", value);
        }
        public static Expression GreaterThanOrEqual(Expression column, Expression value)
        {
            return Condition(column, ">=", value);
        }

        public static Expression GreaterThanOrEqual(string column, object value)
        {
            return Condition(column, ">=", value);
        }

        public static Expression LessThan(Expression column, Expression value)
        {
            return Condition(column, "<", value);
        }

        public static Expression LessThan(string column, object value)
        {
            return Condition(column, "<", value);
        }
        public static Expression LessThanOrEqual(Expression column, Expression value)
        {
            return Condition(column, "<=", value);
        }

        public static Expression LessThanOrEqual(string column, object value)
        {
            return Condition(column, "<=", value);
        }

    }
}