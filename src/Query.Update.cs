using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {


        public Query Update(IEnumerable<string> columns, IEnumerable<object> values)
        {
            if (columns.Count() != values.Count())
            {
                throw new InvalidOperationException("Columns count should be equal to Values count");
            }

            Method = "update";

            for (var i = 0; i < columns.Count(); i++)
            {
                Add("update", new InsertClause
                {
                    Column = columns.ElementAt(i),
                    Value = values.ElementAt(i)
                });
            }

            return this;
        }

        public Query Update(Dictionary<string, object> data)
        {

            Method = "update";

            for (var i = 0; i < data.Count; i++)
            {
                Add("update", new InsertClause
                {
                    Column = data.ElementAt(i).Key,
                    Value = data.ElementAt(i).Value,
                });
            }

            return this;
        }

    }
}