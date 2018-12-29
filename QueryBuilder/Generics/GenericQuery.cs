using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlKata
{
    public partial class Query<T> : Query where T : class
    {
        public Query() : base(typeof(T).Name)
        {
        }

        public Query<T> WhereColumns(Expression<Func<T, object>> expression)
        {
            var exp = expression.GetMemberName().Split();

            WhereColumns(exp[0], exp[1], exp[2]);

            return this;
        }

        public Query<T> WhereFalse(Expression<Func<T, object>> expression)
        {
            WhereFalse(expression.GetMemberName());

            return this;
        }

        public Query<T> WhereNull(Expression<Func<T, object>> expression)
        {
            WhereNull(expression.GetMemberName());

            return this;
        }

        public Query<T> OrWhereNull(Expression<Func<T, object>> expression)
        {
            OrWhereNull(expression.GetMemberName());

            return this;
        }

        public Query<T> OrderBy(params Expression<Func<T, object>>[] columns)
        {
            List<string> members = columns.GetMemberName();

            OrderBy(members.ToArray());

            return this;
        }

        public Query<T> OrderByDesc(params Expression<Func<T, object>>[] columns)
        {
            List<string> members = columns.GetMemberName();

            OrderByDesc(members.ToArray());

            return this;
        }

        public Query<T> GroupBy(Expression<Func<T, object>> expression)
        {
            GroupBy(expression.GetMemberName());

            return this;
        }
    }
}
