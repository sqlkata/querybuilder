using System.Linq.Expressions;

namespace SqlKata.SqlExpressions
{
    public static class Functions
    {
        public static Function Function(string name, SqlExpression body)
        {
            return new Function(name.ToUpperInvariant(), body);
        }

        public static Function Count(SqlExpression body)
        {
            return Function("Count", body);
        }

        public static Function Count(string column)
        {
            return Count(new Identifier(column));
        }

        public static Function Average(SqlExpression body)
        {
            return Function("Avg", body);
        }

        public static Function Average(string column)
        {
            return Average(new Identifier(column));
        }

        public static Function Min(SqlExpression body)
        {
            return Function("Min", body);
        }

        public static Function Min(string column)
        {
            return Min(new Identifier(column));
        }

        public static Function Max(SqlExpression body)
        {
            return Function("Max", body);
        }

        public static Function Max(string column)
        {
            return Max(new Identifier(column));
        }

        public static Case Case(SqlExpression expression = null)
        {
            return new Case(expression);
        }

        public static Function Length(SqlExpression expression)
        {
            return new Function("Length", expression);
        }

        public static Function Length(string column)
        {
            return Length(new Identifier(column));
        }

        public static Function Upper(SqlExpression expression)
        {
            return new Function("Upper", expression);
        }

        public static Function Upper(string column)
        {
            return Upper(new Identifier(column));
        }

        public static Function Lower(SqlExpression expression)
        {
            return new Function("Lower", expression);
        }

        public static Function Lower(string column)
        {
            return Lower(new Identifier(column));
        }

        public static Function Concat(params SqlExpression[] expressions)
        {
            return new Function("Concat", expressions);
        }

        public static Function Concat(params string[] expressions)
        {
            return new Function("Concat", expressions);
        }

        public static Condition Condition(string column, string op, object value)
        {
            if (value is string strValue)
            {
                return new Condition(column, op, new StringValue(strValue));
            }

            return new Condition(column, op, new ParamValue(value));
        }

    }
}