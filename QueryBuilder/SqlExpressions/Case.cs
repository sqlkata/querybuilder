using System.Collections.Generic;

namespace SqlKata.SqlExpressions
{
    public class Case : SqlExpression
    {
        public SqlExpression Test { get; set; }
        public Dictionary<SqlExpression, SqlExpression> Cases { get; set; } = new Dictionary<SqlExpression, SqlExpression>();
        public SqlExpression ElseDefault { get; set; }

        public Case() { }

        public Case(SqlExpression test)
        {
            Test = test;
        }

        public Case When(SqlExpression condition, SqlExpression outcome)
        {
            Cases.Add(condition, outcome);
            return this;
        }

        public Case When(string condition, string outcome)
        {
            Cases.Add(new Literal(condition), new Literal(outcome));
            return this;
        }

        public Case Otherwise(SqlExpression expression)
        {
            ElseDefault = expression;
            return this;
        }

    }
}