using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;
using SqlKata.Contract.CreateTable.DbTableSpecific;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Fillers.CreateTable
{
    internal class OracleCreateQueryFormatFiller : ICreateQueryFormatFiller
    {
        private readonly IColumnCompiler _columnCompiler;
        private readonly IPrimaryKeyCompiler _primaryKeyCompiler;
        private readonly IUniqueConstraintCompiler _uniqueConstraintCompiler;
        private readonly ISqlCreateCommandUtil _sqlCreateCommandUtil;
        private readonly IOracleCreateTableDbExtender _oracleCreateTableDbExtender;

        public OracleCreateQueryFormatFiller(IUniqueConstraintCompiler uniqueConstraintCompiler,
            IPrimaryKeyCompiler primaryKeyCompiler, IColumnCompiler columnCompiler,
            ISqlCreateCommandProvider sqlCreateCommandProvider,
            IOracleCreateTableDbExtender oracleCreateTableDbExtender)
        {
            _uniqueConstraintCompiler = uniqueConstraintCompiler;
            _primaryKeyCompiler = primaryKeyCompiler;
            _columnCompiler = columnCompiler;
            _oracleCreateTableDbExtender = oracleCreateTableDbExtender;
            _sqlCreateCommandUtil = sqlCreateCommandProvider.GetSqlCreateCommandUtil(DataSource.Oracle);
        }

        public DataSource DataSource { get; } = DataSource.Oracle;

        public string FillQueryFormat(string queryFormat, Query query)
        {
            var createTableColumnClauses = query.GetComponents<CreateTableColumn>("CreateTableColumn");
            var tableName = query.GetOneComponent<FromClause>("from").Table;
            var tableType = query.GetOneComponent<TableCluase>("TableType").TableType;
            var tempString = tableType == TableType.Temporary ? _sqlCreateCommandUtil.GetTempTableClause() : "";

            var tableExtensions = query
                .GetOneComponent<CreateTableQueryExtensionClause>("CreateTableExtension");
            var onCommitBehaviour = tableType == TableType.Temporary ? _oracleCreateTableDbExtender.GetOnCommitBehaviour(tableExtensions) : "";

            string hint = "";
            return string.Format(queryFormat,
                tempString,
                hint,
                tableName,
                _columnCompiler.CompileCreateTableColumns(createTableColumnClauses,DataSource.Oracle),
                _primaryKeyCompiler.CompilePrimaryKey(createTableColumnClauses),
                _uniqueConstraintCompiler.CompileUniqueConstraints(createTableColumnClauses),
                onCommitBehaviour
            );
        }
    }
}
