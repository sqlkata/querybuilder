using System.Collections.Generic;

namespace SqlKata.Compilers.Bindings
{
    public class SqlResultBinder : ISqlResultBinder
    {
        protected virtual Dictionary<string, object> PrepareNamedBindings(List<object> bindings)
        {
            var namedParams = new Dictionary<string, object>();

            for (var i = 0; i < bindings.Count; i++)
            {
                namedParams["p" + i] = bindings[i];
            }

            return namedParams;
        }

        protected virtual string PrepareSql(string rawSql)
        {
            return Helper.ReplaceAll(rawSql, "?", x => "@p" + x);
        }
        
        public void BindNamedParameters(SqlResult sqlResult)
        {
            sqlResult.NamedBindings = PrepareNamedBindings(sqlResult.Bindings);
            sqlResult.Sql = PrepareSql(sqlResult.RawSql);
        }
    }
}