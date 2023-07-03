using System.Collections.Generic;
using System.Text;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Contract.CreateTable;

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

        public StringBuilder CompileCreateTable(string tableName,TableType tableType,List<CreateTableColumn> createTableColumnClauses)
        {
            var queryString = new StringBuilder(_sqlCommandUtil.CreateTableCommandGenerator(tableType,tableName));
            queryString.Append("(\n");
            _columnCompiler.CompileCreateTableColumns(queryString,createTableColumnClauses);
            _primaryKeyCompiler.CompilePrimaryKey(queryString,createTableColumnClauses);
            _uniqueConstraintCompiler.CompileUniqueConstraints(queryString,createTableColumnClauses);
            queryString.Append(")\n");
            return queryString;
        }
    }
}
