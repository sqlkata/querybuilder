using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public class SqlResult
    {
        public Query Query { get; set; }
        public string RawSql { get; set; } = "";
        public List<object> Bindings { get; set; } = new List<object>();

        public SqlResult()
        {
        }

        public string Sql
        {
            get
            {
                return Helper.ReplaceAll(RawSql, "?", x => "@p" + x);
            }
        }

        public Dictionary<string, object> NamedBindings
        {
            get
            {
                var namedParams = new Dictionary<string, object>();

                for (var i = 0; i < Bindings.Count; i++)
                {
                    namedParams["p" + i] = Bindings[i];
                }

                return namedParams;
            }
        }

        public override string ToString()
        {
            return Helper.ReplaceAll(RawSql, "?", i =>
            {
                if (i >= Bindings.Count)
                {
                    throw new Exception($"Failed to retrieve a binding at the index {i}, the total bindings count is {Bindings.Count}");
                }

                var value = Bindings[i];

                if (value == null)
                {
                    return "NULL";
                }
                else if (IsNumber(value.ToString()))
                {
                    return value.ToString();
                }
                else if (value is DateTime date)
                {
                    return "'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else if (value is bool vBool)
                {
                    return vBool ? "true" : "false";
                }
                else if (value is Enum vEnum)
                {
                    return ((int)value) + $" /* {vEnum} */";
                }

                // fallback to string
                return "'" + value.ToString() + "'";

            });
        }

        private static bool IsNumber(string val)
        {
            return !string.IsNullOrEmpty(val) && double.TryParse(val, out double num);
        }

        public static SqlResult operator +(SqlResult a, SqlResult b)
        {
            var sql = a.RawSql + ";" + b.RawSql;

            var bindings = a.Bindings.Concat(b.Bindings).ToList();

            var result = new SqlResult
            {
                RawSql = sql,
                Bindings = bindings
            };

            return result;
        }

    }
}