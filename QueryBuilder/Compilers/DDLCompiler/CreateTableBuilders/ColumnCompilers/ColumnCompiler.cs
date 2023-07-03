using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Exceptions.CreateTableQuery;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.ColumnCompilers
{
    internal class ColumnCompiler : IColumnCompiler
    {
        private readonly ISqlCreateCommandUtil _sqlCommandUtil;

        public ColumnCompiler(ISqlCreateCommandUtil sqlCommandUtil)
        {
            _sqlCommandUtil = sqlCommandUtil;
        }
        public void CompileCreateTableColumns(StringBuilder queryString, List<CreateTableColumn> createTableColumnClauses)
        {
            var identityAndAutoIncrementColumns = createTableColumnClauses.Where(x => x.IsIdentity || x.IsAutoIncrement);
            if (identityAndAutoIncrementColumns.Count() > 1)
            {
                throw new AutoIncrementOrIdentityExceededException("table can not have more than one auto increment or identity column");
            }
            foreach (var columnCluase in createTableColumnClauses)
            {
                var nullOrNot = columnCluase.IsNullable ? "NULL " : "NOT NULL ";
                if (columnCluase.IsIdentity || columnCluase.IsAutoIncrement)
                {
                    queryString.Append($"{columnCluase.ColumnName} {columnCluase.ColumnDbType.GetDBType()}  {_sqlCommandUtil.AutoIncrementIdentityCommandGenerator()},\n");
                    continue;
                }
                queryString.Append($"{columnCluase.ColumnName} {columnCluase.ColumnDbType.GetDBType()} {nullOrNot},\n");
            }
        }
    }
}
