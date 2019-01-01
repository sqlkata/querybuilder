using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace SqlKata
{
    public partial class Query<T> : Query where T : class
    {
        public Query<T> Select(Expression<Func<T, object>> columns)
        {
            Select(columns.GetMemberNames());

            return this;
        }
    }
}
