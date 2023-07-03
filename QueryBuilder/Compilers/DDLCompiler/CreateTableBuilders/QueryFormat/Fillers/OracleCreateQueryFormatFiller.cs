using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Fillers
{
    internal class OracleCreateQueryFormatFiller : ICreateQueryFormatFiller
    {
        private readonly IColumnCompiler _columnCompiler;
        private readonly IPrimaryKeyCompiler _primaryKeyCompiler;
        private readonly IUniqueConstraintCompiler _uniqueConstraintCompiler;

        public OracleCreateQueryFormatFiller(IUniqueConstraintCompiler uniqueConstraintCompiler, IPrimaryKeyCompiler primaryKeyCompiler, IColumnCompiler columnCompiler)
        {
            _uniqueConstraintCompiler = uniqueConstraintCompiler;
            _primaryKeyCompiler = primaryKeyCompiler;
            _columnCompiler = columnCompiler;
        }

        public DataSource DataSource { get; } = DataSource.Oracle;
        public string FillQueryFormat(string queryFormat,Query query)
        {
            var createTableColumnClauses = query.GetComponents<CreateTableColumn>("CreateTableColumn");
            var tableName = query.GetOneComponent<FromClause>("from").Table;
            var tableType = query.GetOneComponent<TableCluase>("TableType").TableType;
            var tempString = tableType == TableType.Temporary ? "GLOBAL TEMPORARY" : "";
            string hint = "";
            return string.Format(queryFormat,
                tempString,
                hint,
                tableName,
                _columnCompiler.CompileCreateTableColumns(createTableColumnClauses),
                _primaryKeyCompiler.CompilePrimaryKey(createTableColumnClauses),
                _uniqueConstraintCompiler.CompileUniqueConstraints(createTableColumnClauses)
                );
        }
    }
}
