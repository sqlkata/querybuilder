using System;
using System.Linq.Expressions;
using SqlKata.SqlExpressions;

namespace SqlKata.Compilers.Visitors
{
    public interface SqlExpressionVisitorInterface
    {
        // string Visit(AbstractSqlExpression expression);
        string Visit(StringValue expression);
        string Visit(Concat expression);
        string Visit(Count expression);
        string Visit(JsonExtract expression);
        string Visit(Lower expression);
        string Visit(Upper expression);
        string Visit(Length expression);
        string Visit(Cast expression);
        string Visit(Identifier expression);
        string Visit(Literal expression);
        string Visit(Function expression);
        string Visit(Expression expression);
        string Visit(BinaryExpression expression);
        string Visit(ConstantExpression expression);
        string Visit(UnaryExpression expression);
        string Visit(ParameterExpression expression);
        string Visit(ConditionalExpression expression);
        string Visit(Expression<Func<bool>> expression);
        string Visit(Wrap expression);
        string Visit(Case expression);
        string Visit(BlockExpression expression);
    }
}