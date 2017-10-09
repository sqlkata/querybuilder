using SqlKata.QueryBuilder.Clauses;

namespace SqlKata.QueryBuilder
{
    public partial class Query
    {
        public Query Having<T>(string column, string op, T value)
        {

            // If the value is "null", we will just assume the developer wants to add a
            // having null clause to the query. So, we will allow a short-cut here to
            // that method for convenience so the developer doesn't have to check.
            if (value == null)
            {
                return Not(op != "=").HavingNull(column);
            }

            return Add("having", new BasicCondition<T>
            {
                Column = column,
                Operator = op,
                Value = value,
                IsOr = getOr(),
                IsNot = getNot()
            });
        }

        public Query OrHaving(string column, string op, object value)
        {
            return Or().Having(column, op, value);
        }

        public Query HavingNot(string column, string op, object value)
        {
            return Not(true).Having(column, op, value);
        }

        public Query OrHavingNot(string column, string op, object value)
        {
            return Or().Not(true).Having(column, op, value);
        }

        public Query HavingNull(string column)
        {
            return Add("having", new NullCondition
            {
                Column = column,
                IsOr = getOr(),
                IsNot = getNot(),
            });
        }

        public Query OrHavingNull(string column)
        {
            return Or().HavingNull(column);
        }

        public Query HavingNotNull(string column)
        {
            return Not(true).HavingNull(column);
        }

        public Query OrHavingNotNull(string column)
        {
            return Or().Not(true).HavingNull(column);
        }

        public Query HavingRaw(string expression, params object[] bindings)
        {
            Add("having", new RawCondition
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });

            return this;
        }

        public Query OrHavingRaw(string expression, params object[] bindings)
        {
            return Or().HavingRaw(expression, bindings);
        }
    }
}
