using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public class SqlResult
    {
        public string RawSql { get; set; }
        public List<object> RawBindings { get; set; }

        public SqlResult(string sql, List<object> bindings)
        {
            RawSql = sql;
            RawBindings = bindings;
        }

        public string Sql
        {
            get
            {
                return Helper.ReplaceAll(RawSql, "?", x => "@p" + x);
            }
        }

        public Dictionary<string, object> Bindings
        {
            get
            {
                var namedParams = new Dictionary<string, object>();

                for (var i = 0; i < RawBindings.Count; i++)
                {
                    namedParams["p" + i] = RawBindings[i];
                }

                return namedParams;
            }
        }

        public override string ToString()
        {
            return Helper.ReplaceAll(RawSql, "?", i =>
            {
                var value = RawBindings[i];

                if (value == null)
                {
                    return "NULL";
                }

                if (isNumber(value))
                {
                    return value + "";
                }

                return $"'{value}'";

            });
        }

        private bool isNumber(object val)
        {
            if (val == null) return false;

            return val.ToString().All(x => char.IsDigit(x));
        }

    }
}