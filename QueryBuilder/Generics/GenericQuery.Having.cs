using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SqlKata
{
    public partial class Query<T> : Query where T : class
    {
        public Query<T> Having(Expression<Func<T, object>> expression)
        {
            var exp = expression.GetMemberName().Split();

            Having(exp[0], exp[1], exp[2]);

            return this;
        }

        public Query<T> HavingNot(Expression<Func<T, object>> expression)
        {
            var exp = expression.GetMemberName().Split();

            HavingNot(exp[0], exp[1], exp[2]);

            return this;
        }

        public Query<T> OrHaving(Expression<Func<T, object>> expression)
        {
            var exp = expression.GetMemberName().Split();

            OrHaving(exp[0], exp[1], exp[2]);

            return this;
        }

        public Query<T> OrHavingNot(Expression<Func<T, object>> expression)
        {
            var exp = expression.GetMemberName().Split();

            OrHavingNot(exp[0], exp[1], exp[2]);

            return this;
        }
    }
}
