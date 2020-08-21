using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlKata.SqlExpressions
{
    public class Wrap : AbstractSqlExpression
    {
        public Expression Body { get; }
        public Wrap(Expression body)
        {
            Body = body;
        }

    }
}