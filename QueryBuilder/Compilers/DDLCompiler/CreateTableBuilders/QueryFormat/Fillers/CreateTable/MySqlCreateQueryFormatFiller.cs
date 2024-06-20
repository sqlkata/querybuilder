using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Fillers.CreateTable
{
    internal class MySqlCreateQueryFormatFiller : ICreateQueryFormatFiller
    {
        private readonly IColumnCompiler _columnCompiler;
        private readonly IPrimaryKeyCompiler _primaryKeyCompiler;
        private readonly IUniqueConstraintCompiler _uniqueConstraintCompiler;
        private readonly ISqlCreateCommandUtil _createCommandUtil;

        public MySqlCreateQueryFormatFiller(IColumnCompiler columnCompiler, IPrimaryKeyCompiler primaryKeyCompiler, IUniqueConstraintCompiler uniqueConstraintCompiler, ISqlCreateCommandProvider createCommandProvider)
        {
            _columnCompiler = columnCompiler;
            _primaryKeyCompiler = primaryKeyCompiler;
            _uniqueConstraintCompiler = uniqueConstraintCompiler;
            _createCommandUtil = createCommandProvider.GetSqlCreateCommandUtil(DataSource.MySql);
        }

        public DataSource DataSource { get; } = DataSource.MySql;
        public string FillQueryFormat(string queryFormat,Query query)
        {
            var createTableColumnClauses = query.GetComponents<CreateTableColumn>("CreateTableColumn");
            var tableName = query.GetOneComponent<FromClause>("from").Table;
            var tableType = query.GetOneComponent<TableCluase>("TableType").TableType;
            var tempString = tableType == TableType.Temporary ? _createCommandUtil.GetTempTableClause() : "";
            return string.Format(queryFormat,
                tempString,
                tableName,
                _columnCompiler.CompileCreateTableColumns(createTableColumnClauses,DataSource.MySql),
                _primaryKeyCompiler.CompilePrimaryKey(createTableColumnClauses),
                _uniqueConstraintCompiler.CompileUniqueConstraints(createTableColumnClauses)
            );
        }
    }
}
