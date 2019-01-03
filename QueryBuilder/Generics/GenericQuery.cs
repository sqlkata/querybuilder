using System;
using System.Linq.Expressions;

namespace SqlKata
{
    public partial class Query<T> : Query where T : class
    {
        public Query() : base(typeof(T).Name)
        {
            TableName = typeof(T).Name;
        }

        public new Query<T> From(string table)
        {
            TableName = table;

            base.From(table);

            return this;
        }

        public string TableName { get; private set; } = "";

        public Query<T> OrderBy(Expression<Func<T, object>> columns)
        {
            OrderBy(columns.GetMemberNames());

            return this;
        }

        public Query<T> OrderByDesc(Expression<Func<T, object>> columns)
        {
            OrderByDesc(columns.GetMemberNames());

            return this;
        }

        public Query<T> GroupBy(Expression<Func<T, object>> expression)
        {
            GroupBy(expression.GetMemberName());

            return this;
        }
    }
}
