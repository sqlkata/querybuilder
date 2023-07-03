using System.Text;
using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableCompilers
{
    internal class GeneralCreateTableCompiler : ICreateTableQueryCompiler
    {
        private readonly ISqlCreateCommandUtil _sqlCommandUtil;
        private readonly IColumnCompiler _columnCompiler;
        private readonly IPrimaryKeyCompiler _primaryKeyCompiler;
        private readonly IUniqueConstraintCompiler _uniqueConstraintCompiler;

        public GeneralCreateTableCompiler(ISqlCreateCommandUtil sqlCommandUtil, IColumnCompiler columnCompiler, IPrimaryKeyCompiler primaryKeyCompiler, IUniqueConstraintCompiler uniqueConstraintCompiler)
        {
            _sqlCommandUtil = sqlCommandUtil;
            _columnCompiler = columnCompiler;
            _primaryKeyCompiler = primaryKeyCompiler;
            _uniqueConstraintCompiler = uniqueConstraintCompiler;
        }

        public string CompileCreateTable(Query query)
        {
            /*var createTableColumnClauses = query.GetComponents<CreateTableColumn>("CreateTableColumn");
            var tableName = query.GetOneComponent<FromClause>("from").Table;
            var tableType = query.GetOneComponent<TableCluase>("TableType").TableType;

            var createTableString = _sqlCommandUtil.CreateTableCommandGenerator(tableType, tableName);
            var queryString = new StringBuilder(createTableString);
            queryString.Append("(\n");
            _columnCompiler.CompileCreateTableColumns(queryString,createTableColumnClauses);
            _primaryKeyCompiler.CompilePrimaryKey(queryString,createTableColumnClauses);
            _uniqueConstraintCompiler.CompileUniqueConstraints(queryString,createTableColumnClauses);
            queryString.Append(")\n");*/

            /*
            return RefineQueryString(queryString.ToString());*/
            return null;
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
