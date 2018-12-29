using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace SqlKata
{
    public partial class Query<T> : Query where T : class
    {
        public Query<T> Select(params Expression<Func<T, object>>[] columns)
        {
            List<string> members = columns.GetMemberName();

            Select(members.ToArray());

            return this;
        }
    }
}
