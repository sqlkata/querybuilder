using System.Linq.Expressions;

namespace SqlKata.SqlExpressions
{
    public class Wrap : SqlExpression
    {
        public Expression Body { get; }
        public Wrap(Expression body)
        {
            Body = body;
        }

    }
}