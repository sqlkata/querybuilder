using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {

        public Query Select(params string[] columns)
        {
            return Select(columns.AsEnumerable());
        }

        public Query Select(IEnumerable<string> columns)
        {
            Method = "select";

            columns = columns
                .Select(x => Helper.ExpandExpression(x))
                .SelectMany(x => x)
                .ToArray();


            foreach (var column in columns)
            {
                AddComponent("select", new Column
                {
                    Name = column
                });
            }

            return this;
        }

        /// <summary>
        /// Add a new "raw" select expression to the query.
        /// </summary>
        /// <returns></returns>
        public Query SelectRaw(string sql, params object[] bindings)
        {
            Method = "select";

            AddComponent("select", new RawColumn
            {
                Expression = sql,
                Bindings = bindings,
            });

            return this;
        }

        public Query Select(Query query, string alias)
        {
            Method = "select";

            query = query.Clone();

            AddComponent("select", new QueryColumn
            {
                Query = query.As(alias),
            });

            return this;
        }

        public Query Select(Func<Query, Query> callback, string alias)
        {
            return Select(callback.Invoke(NewChild()), alias);
        }

        public Query SelectAggregate(string aggregate, string column, Query filter = null)
        {
            Method = "select";

            AddComponent("select", new AggregatedColumn
            {
                Column = new Column { Name = column },
                Aggregate = aggregate,
                Filter = filter,
            });

            return this;
        }

        public Query SelectAggregate(string aggregate, string column, Func<Query, Query> filter)
        {
            if (filter == null)
            {
                return SelectAggregate(aggregate, column);
            }

            return SelectAggregate(aggregate, column, filter.Invoke(NewChild()));
        }

        public Query SelectSum(string column, Func<Query, Query> filter = null)
        {
            return SelectAggregate("sum", column, filter);
        }

        public Query SelectCount(string column, Func<Query, Query> filter = null)
        {
            return SelectAggregate("count", column, filter);
        }

        public Query SelectAvg(string column, Func<Query, Query> filter = null)
        {
            return SelectAggregate("avg", column, filter);
        }

        public Query SelectMin(string column, Func<Query, Query> filter = null)
        {
            return SelectAggregate("min", column, filter);
        }

        public Query SelectMax(string column, Func<Query, Query> filter = null)
        {
            return SelectAggregate("max", column, filter);
        }

        #region date
        public Query SelectDatePart(string part, string column,string alias)
        {
            return AddComponent("select", new DateQueryColumn
            {
                Name = alias, 
                Part = part?.ToLowerInvariant(),
                Column=column
            });
        }
        public Query SelectNotDatePart(string part, string column, string alias)
        {
            return Not().SelectDatePart(part, column, alias);
        }

        public Query OrSelectDatePart(string part, string column, string alias)
        {
            return Or().SelectDatePart(part, column, alias);
        }

        public Query OrSelectNotDatePart(string part, string column, string alias)
        {
            return Or().Not().SelectDatePart(part, column, alias);
        }

        public Query SelectDate(string column, string alias)
        {
            return SelectDatePart("date", column, alias);
        }
        public Query SelectNotDate(string column, string alias)
        {
            return Not().SelectDate(column, alias);
        }
        public Query OrSelectDate(string column, string alias)
        {
            return Or().SelectDate(column, alias);
        }
        public Query OrSelectNotDate(string column, string alias)
        {
            return Or().Not().SelectDate(column, alias);
        }

        public Query SelectTime(string column, string alias)
        {
            return SelectDatePart("time", column, alias);
        }
        public Query SelectNotTime(string column, string alias)
        {
            return Not().SelectTime(column, alias);
        }
        public Query OrSelectTime(string column, string alias)
        {
            return Or().SelectTime(column, alias);
        }
        public Query OrSelectNotTime(string column, string alias)
        {
            return Or().Not().SelectTime(column, alias);
        }
        #endregion

    }
}
