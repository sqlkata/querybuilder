using System.Collections.Generic;

namespace SqlKata
{
    public class SqlResult
    {
        public SqlResult(string sql, List<object> bindings)
        {
            Sql = sql;
            Bindings = bindings;
        }

        public string Sql { get; set; }
        public List<object> Bindings { get; set; }
    }
}