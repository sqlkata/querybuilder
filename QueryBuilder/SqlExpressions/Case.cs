using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlKata.SqlExpressions
{
    public class Case : AbstractSqlExpression
    {
        public Expression Test { get; set; }
        public Dictionary<Expression, Expression> Cases { get; set; } = new Dictionary<Expression, Expression>();
        public Expression ElseDefault { get; set; }

        public Case() { }

        public Case(Expression test)
        {
            Test = test;
        }

        public Case When(Expression condition, Expression outcome)
        {
            Cases.Add(condition, outcome);
            return this;
        }

        public Case When(string condition, string outcome)
        {
            Cases.Add(new Literal(condition), new Literal(outcome));
            return this;
        }

        public Case Otherwise(Expression expression)
        {
            ElseDefault = expression;
            return this;
        }

    }
}