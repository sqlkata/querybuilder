using SqlKata.Compilers.DDLCompiler.Abstractions;

namespace SqlKata.Compilers.DDLCompiler.DeleteDdl
{
    public class TruncateTableQueryFactory : ITruncateTableQueryFactory
    {
        public string CompileQuery(Query query)
        {
            var tableName = query.GetOneComponent<FromClause>("from").Table;
            return $"Truncate Table {tableName}";
        }
    }
}
