using System.Linq.Expressions;

namespace SqlKata.SqlExpressions
{
    public static class Functions
    {
        public static Function Function(string name, Expression body)
        {
            return new Function(name.ToUpperInvariant(), body);
        }

        public static Function Count(Expression body)
        {
            return Function("Count", body);
        }

        public static Function Count(string column)
        {
            return Count(new Identifier(column));
        }

        public static Function Average(Expression body)
        {
            return Function("Avg", body);
        }

        public static Function Average(string column)
        {
            return Average(new Identifier(column));
        }

        public static Function Min(Expression body)
        {
            return Function("Min", body);
        }

        public static Function Min(string column)
        {
            return Min(new Identifier(column));
        }

        public static Function Max(Expression body)
        {
            return Function("Max", body);
        }

        public static Function Max(string column)
        {
            return Max(new Identifier(column));
        }

        public static Case Case(Expression expression = null)
        {
            return new Case(expression);
        }

        public static Function Length(Expression expression)
        {
            return new Function("Length", expression);
        }

        public static Function Length(string column)
        {
            return Length(new Identifier(column));
        }

        public static Function Upper(Expression expression)
        {
            return new Function("Upper", expression);
        }

        public static Function Upper(string column)
        {
            return Upper(new Identifier(column));
        }

        public static Function Lower(Expression expression)
        {
            return new Function("Lower", expression);
        }

        public static Function Lower(string column)
        {
            return Lower(new Identifier(column));
        }

        public static Function Concat(params Expression[] expressions)
        {
            return new Function("Concat", expressions);
        }

        public static Function Concat(params string[] expressions)
        {
            return new Function("Concat", expressions);
        }

    }
}