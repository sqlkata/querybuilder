using SqlKata.Compilers.DDLCompiler.Abstractions;

namespace SqlKata.Compilers.DDLCompiler.DeleteDdl
{
    public class DropTableQueryFactory : IDropTableQueryFactory
    {
        public string CompileQuery(Query query)
        {
            var tableName = query.GetOneComponent<FromClause>("from").Table;
            return $"Drop Table {tableName}";
        }
    }
}
