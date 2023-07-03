using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Fillers
{
    internal class MySqlCreateQueryFormatFiller : ICreateQueryFormatFiller
    {
        private readonly IColumnCompiler _columnCompiler;
        private readonly IPrimaryKeyCompiler _primaryKeyCompiler;
        private readonly IUniqueConstraintCompiler _uniqueConstraintCompiler;

        public MySqlCreateQueryFormatFiller(IColumnCompiler columnCompiler, IPrimaryKeyCompiler primaryKeyCompiler, IUniqueConstraintCompiler uniqueConstraintCompiler)
        {
            _columnCompiler = columnCompiler;
            _primaryKeyCompiler = primaryKeyCompiler;
            _uniqueConstraintCompiler = uniqueConstraintCompiler;
        }

        public DataSource DataSource { get; } = DataSource.MySql;
        public string FillQueryFormat(string queryFormat,Query query)
        {
            var createTableColumnClauses = query.GetComponents<CreateTableColumn>("CreateTableColumn");
            var tableName = query.GetOneComponent<FromClause>("from").Table;
            var tableType = query.GetOneComponent<TableCluase>("TableType").TableType;
            var tempString = tableType == TableType.Temporary ? "TEMPORARY" : "";
            return string.Format(queryFormat,
                tempString,
                tableName,
                _columnCompiler.CompileCreateTableColumns(createTableColumnClauses),
                _primaryKeyCompiler.CompilePrimaryKey(createTableColumnClauses),
                _uniqueConstraintCompiler.CompileUniqueConstraints(createTableColumnClauses)
            );
        }
    }
}
