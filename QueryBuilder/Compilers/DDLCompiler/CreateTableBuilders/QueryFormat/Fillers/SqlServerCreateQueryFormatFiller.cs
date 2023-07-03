using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Fillers
{
    internal class SqlServerCreateQueryFormatFiller : ICreateQueryFormatFiller
    {
        private readonly IColumnCompiler _columnCompiler;
        private readonly IPrimaryKeyCompiler _primaryKeyCompiler;
        private readonly IUniqueConstraintCompiler _uniqueConstraintCompiler;

        public SqlServerCreateQueryFormatFiller(IColumnCompiler columnCompiler, IPrimaryKeyCompiler primaryKeyCompiler, IUniqueConstraintCompiler uniqueConstraintCompiler)
        {
            _columnCompiler = columnCompiler;
            _primaryKeyCompiler = primaryKeyCompiler;
            _uniqueConstraintCompiler = uniqueConstraintCompiler;
        }

        public DataSource DataSource { get; } = DataSource.SqlServer;
        public string FillQueryFormat(string queryFormat,Query query)
        {
            var createTableColumnClauses = query.GetComponents<CreateTableColumn>("CreateTableColumn");
            var tableName = query.GetOneComponent<FromClause>("from").Table;
            return string.Format(queryFormat,
                tableName,
                _columnCompiler.CompileCreateTableColumns(createTableColumnClauses),
                _primaryKeyCompiler.CompilePrimaryKey(createTableColumnClauses),
                _uniqueConstraintCompiler.CompileUniqueConstraints(createTableColumnClauses)
                );
        }
    }
}
