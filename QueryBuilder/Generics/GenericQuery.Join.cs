using System;
using System.Linq.Expressions;

namespace SqlKata
{
    public partial class Query<T> : Query where T : class
    {
        public Query<T> Join<JTable>(Expression<Func<T, JTable, object>> expression) where JTable : class
        {
            Join(null, expression);

            return this;
        }

        public Query<T> Join<JTable>(string tableName, Expression<Func<T, JTable, object>> expression) where JTable : class
        {
            var joinColumns = expression.GetMemberNames();

            if (tableName != null && tableName.Length > 0)
            {
                Join(tableName, joinColumns[0], joinColumns[1]);
            }
            else
            {
                Join(typeof(JTable).Name, joinColumns[0], joinColumns[1]);
            }

            return this;
        }

        public Query<T> LeftJoin<JTable>(Expression<Func<T, JTable, object>> expression) where JTable : class
        {
            LeftJoin(null, expression);

            return this;
        }

        public Query<T> LeftJoin<JTable>(string tableName, Expression<Func<T, JTable, object>> expression) where JTable : class
        {
            var joinColumns = expression.GetMemberNames();

            if (tableName != null && tableName.Length > 0)
            {
                LeftJoin(tableName, joinColumns[0], joinColumns[1]);
            }
            else
            {
                LeftJoin(typeof(JTable).Name, joinColumns[0], joinColumns[1]);
            }

            return this;
        }

        public Query<T> RightJoin<JTable>(Expression<Func<T, JTable, object>> expression) where JTable : class
        {
            RightJoin(null, expression);

            return this;
        }

        public Query<T> RightJoin<JTable>(string tableName, Expression<Func<T, JTable, object>> expression) where JTable : class
        {
            var joinColumns = expression.GetMemberNames();

            if (tableName != null && tableName.Length > 0)
            {
                RightJoin(tableName, joinColumns[0], joinColumns[1]);
            }
            else
            {
                RightJoin(typeof(JTable).Name, joinColumns[0], joinColumns[1]);
            }

            return this;
        }

        public Query<T> CrossJoin<JTable>() where JTable : class
        {
            CrossJoin<JTable>(null);

            return this;
        }

        public Query<T> CrossJoin<JTable>(string tableName) where JTable : class
        {
            if (tableName != null && tableName.Length > 0)
            {
                CrossJoin(tableName);
            }
            else
            {
                CrossJoin(typeof(JTable).Name);
            }

            return this;
        }
    }
}
