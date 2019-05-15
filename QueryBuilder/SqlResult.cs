using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public class SqlResult
    {
        public Query Query { get; set; }
        public string RawSql { get; set; } = "";
        public List<object> Bindings { get; set; } = new List<object>();
        public string Sql { get; set; } = "";
        public Dictionary<string, object> NamedBindings = new Dictionary<string, object>();

        private static readonly Type[] NumberTypes =
        {
            typeof(int),
            typeof(long),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(short),
            typeof(ushort),
            typeof(ulong),
        };

        public override string ToString()
        {
            var deepParameters = GetParametersWithoutVars();

            return Helper.ReplaceAll(ReplaceVariables(RawSql), "?", i =>
            {
                if (i >= deepParameters.Count)
                {
                    throw new Exception(
                        $"Failed to retrieve a binding at the index {i}, the total bindings count is {Bindings.Count}");
                }

                var value = deepParameters[i];
                return ChangeToSqlValue(value);
            });
        }

        public void CopyClauses<C>(SqlResult subCtx, string name, string engineCode = null) where C : AbstractClause
        {
            var clauses = subCtx.Query.GetComponents<C>(name, engineCode);
            Query.Clauses.AddRange(clauses);
        }

        private string ReplaceVariables(string rawSqlquery)
        {
            var withVars = Query.GetComponents<WithVarClause>("withVar");
            foreach (var variable in withVars)
            {
                object value = variable.Value;
                string valueSql = ChangeToSqlValue(value);
                rawSqlquery = rawSqlquery.Replace(variable.Name, valueSql);
            }
            return rawSqlquery;
        }


        private string ChangeToSqlValue(object value)
        {

            if (value == null)
            {
                return "NULL";
            }

            if (Helper.IsArray(value))
            {
                return Helper.JoinArray(",", value as IEnumerable);
            }

            if (NumberTypes.Contains(value.GetType()))
            {
                return value.ToString();
            }

            if (value is DateTime date)
            {
                if (date.Date == date)
                {
                    return "'" + date.ToString("yyyy-MM-dd") + "'";
                }

                return "'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            }

            if (value is bool vBool)
            {
                return vBool ? "true" : "false";
            }

            if (value is Enum vEnum)
            {
                return Convert.ToInt32(vEnum) + $" /* {vEnum} */";
            }

            // fallback to string
            return "'" + value.ToString() + "'";
        }


        /// <summary>
        /// Additional helper method in order to avoid assign wrong parameters in the to string method.
        /// </summary>
        /// <returns></returns>
        private List<object> GetParametersWithoutVars()
        {

            var withVars = Query.GetComponents<WithVarClause>("withVar");
            return Helper.Flatten(Bindings).
                                Where(x => !withVars.Any(k => k.Name.Equals(x.ToString(), StringComparison.OrdinalIgnoreCase))).ToList();
        }


    }
}