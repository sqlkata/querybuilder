using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;

namespace SqlKata.Compilers.DDLCompiler
{
    internal class DDLCompiler
    {
        private readonly ICreateTableQueryCompiler _createTableQueryCompiler;

        public DDLCompiler(ICreateTableQueryCompiler createTableQueryCompiler)
        {
            _createTableQueryCompiler = createTableQueryCompiler;
        }


        internal SqlResult CompileCreateTable(Query query)
        {
            var result = new SqlResult()
            {
                Query = query.Clone(),
            };
            var createTableColumnClauses = result.Query.GetComponents<CreateTableColumn>("CreateTableColumn");
            var tableName = result.Query.GetOneComponent<FromClause>("from").Table;
            var tableType = result.Query.GetOneComponent<TableCluase>("TableType").TableType;
            var queryString = _createTableQueryCompiler.CompileCreateTable(tableName,tableType,createTableColumnClauses);
            result.RawSql = RefineQueryString(queryString.ToString());
            return result;
        }

        private static string RefineQueryString(string queryString)
        {
            var lastCommaChar = queryString.LastIndexOf(',');
            if(lastCommaChar != -1)
                queryString = queryString.Remove(lastCommaChar,1);
            return queryString;
        }
    }
}
