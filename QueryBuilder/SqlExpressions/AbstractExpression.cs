using System;
using System.Linq.Expressions;
using SqlKata.Compilers;

namespace SqlKata.SqlExpressions
{
    public abstract class SqlExpression
    {
        public string Accept(Compiler visitor)
        {
            return visitor.Visit((dynamic)this);
        }
    }
}