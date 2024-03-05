using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Fillers.CreateTable
{
    internal class PostgresqlCreateQueryFormatFiller : ICreateQueryFormatFiller
    {
        private readonly IColumnCompiler _columnCompiler;
        private readonly IPrimaryKeyCompiler _primaryKeyCompiler;
        private readonly IUniqueConstraintCompiler _uniqueConstraintCompiler;
        private readonly ISqlCreateCommandUtil _sqlCreateCommandUtil;

        public PostgresqlCreateQueryFormatFiller(IColumnCompiler columnCompiler, IPrimaryKeyCompiler primaryKeyCompiler,
            IUniqueConstraintCompiler uniqueConstraintCompiler, ISqlCreateCommandProvider sqlCreateCommandProvider)
        {
            _columnCompiler = columnCompiler;
            _primaryKeyCompiler = primaryKeyCompiler;
            _uniqueConstraintCompiler = uniqueConstraintCompiler;
            _sqlCreateCommandUtil = sqlCreateCommandProvider.GetSqlCreateCommandUtil(DataSource.Postgresql);
        }

        public DataSource DataSource { get; } = DataSource.Postgresql;

        public string FillQueryFormat(string queryFormat, Query query)
        {
            var createTableColumnClauses = query.GetComponents<CreateTableColumn>("CreateTableColumn");
            var tableName = query.GetOneComponent<FromClause>("from").Table;
            var tableType = query.GetOneComponent<TableCluase>("TableType").TableType;
            var tempString = tableType == TableType.Temporary ? _sqlCreateCommandUtil.GetTempTableClause() : "";
            return string.Format(queryFormat,
                tempString,
                tableName,
                _columnCompiler.CompileCreateTableColumns(createTableColumnClauses,DataSource.Postgresql),
                _primaryKeyCompiler.CompilePrimaryKey(createTableColumnClauses),
                _uniqueConstraintCompiler.CompileUniqueConstraints(createTableColumnClauses)
            );
        }
    }
}
