using System.Linq.Expressions;

namespace SqlKata.SqlExpressions
{
    public class Length : AbstractSqlExpression
    {
        public Expression Value { get; }

        public Length(Expression expression)
        {
            this.Value = expression;
        }

        public Length(string column)
        {
            this.Value = new Identifier(column);
        }
    }
}