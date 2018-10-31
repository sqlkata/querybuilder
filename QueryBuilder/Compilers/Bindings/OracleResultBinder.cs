namespace SqlKata.Compilers.Bindings
{
    public class OracleResultBinder : SqlResultBinder
    {
        protected override string PrepareSql(string rawSql)
        {
            return Helper.ReplaceAll(rawSql, "?", x => ":p" + x);
        }
    }
}