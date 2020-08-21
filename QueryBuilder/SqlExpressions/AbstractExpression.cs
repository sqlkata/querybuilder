using System;
using System.Linq.Expressions;
using SqlKata.Compilers.Visitors;

namespace SqlKata.SqlExpressions
{
    public abstract class AbstractSqlExpression : Expression
    {
        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => typeof(AbstractSqlExpression);
        public string Accept(SqlExpressionVisitorInterface visitor)
        {
            return visitor.Visit((dynamic)this);
        }
    }
}