using System;
using System.Linq.Expressions;

namespace SqlKata
{
    public partial class Query<T> : Query where T : class
    {
        public Query<T> Where(Expression<Func<T, object>> expression)
        {
            var exp = expression.GetMemberName().Split();

            Where(exp[0], exp[1], exp[2]);

            return this;
        }

        public Query<T> WhereNot(Expression<Func<T, object>> expression)
        {
            var exp = expression.GetMemberName().Split();

            WhereNot(exp[0], exp[1], exp[2]);

            return this;
        }

        public Query<T> OrWhere(Expression<Func<T, object>> expression)
        {
            var exp = expression.GetMemberName().Split();

            OrWhere(exp[0], exp[1], exp[2]);

            return this;
        }

        public Query<T> OrWhereNot(Expression<Func<T, object>> expression)
        {
            var exp = expression.GetMemberName().Split();

            OrWhereNot(exp[0], exp[1], exp[2]);

            return this;
        }

        public Query<T> OrWhereColumns(Expression<Func<T, object>> expression)
        {
            var exp = expression.GetMemberName().Split();

            OrWhereColumns(exp[0], exp[1], exp[2]);

            return this;
        }

        public Query<T> WhereNull(Expression<Func<T, object>> expression)
        {
            WhereNull(expression.GetMemberName());

            return this;
        }

        public Query<T> WhereNotNull(Expression<Func<T, object>> expression)
        {
            WhereNotNull(expression.GetMemberName());

            return this;
        }

        public Query<T> OrWhereNull(Expression<Func<T, object>> expression)
        {
            OrWhereNull(expression.GetMemberName());

            return this;
        }

        public Query<T> OrWhereNotNull(Expression<Func<T, object>> expression)
        {
            OrWhereNotNull(expression.GetMemberName());

            return this;
        }

        public Query<T> WhereTrue(Expression<Func<T, object>> expression)
        {
            WhereTrue(expression.GetMemberName());

            return this;
        }

        public Query<T> OrWhereTrue(Expression<Func<T, object>> expression)
        {
            OrWhereTrue(expression.GetMemberName());

            return this;
        }

        public Query<T> WhereFalse(Expression<Func<T, object>> expression)
        {
            WhereFalse(expression.GetMemberName());

            return this;
        }

        public Query<T> OrWhereFalse(Expression<Func<T, object>> expression)
        {
            OrWhereFalse(expression.GetMemberName());

            return this;
        }
    }

}
