using System.Linq.Expressions;

namespace SqlKata.SqlExpressions
{
    public static class Common
    {
        public static Literal Literal(object value)
        {
            return new Literal(value);
        }

        public static Identifier Identifier(string value)
        {
            return new Identifier(value);
        }

        public static StringValue StringValue(string value)
        {
            return new StringValue(value);
        }
    }
}