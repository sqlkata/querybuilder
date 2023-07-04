using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Exceptions.CreateTableQuery;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.ColumnCompilers
{
    internal class ColumnCompiler : IColumnCompiler
    {
        private readonly ISqlCreateCommandProvider _sqlCreateCommandProvider;

        public ColumnCompiler(ISqlCreateCommandProvider sqlCreateCommandProvider)
        {
            this._sqlCreateCommandProvider = sqlCreateCommandProvider;
        }
        public string CompileCreateTableColumns(List<CreateTableColumn> createTableColumnClauses,DataSource dataSource)
        {
            var queryString = new StringBuilder();
            var identityAndAutoIncrementColumns = createTableColumnClauses.Where(x => x.IsIdentity || x.IsAutoIncrement);
            if (identityAndAutoIncrementColumns.Count() > 1)
            {
                throw new AutoIncrementOrIdentityExceededException("table can not have more than one auto increment or identity column");
            }
            foreach (var columnClause in createTableColumnClauses)
            {
                var nullOrNot = columnClause.IsNullable ? "NULL " : "NOT NULL ";
                var collate = columnClause.Collate == null ? "" : $"Collate {columnClause.Collate}";
                if (columnClause.IsIdentity || columnClause.IsAutoIncrement)
                {
                    queryString.Append($"{columnClause.ColumnName} {columnClause.ColumnDbType.GetDBType()} {collate} {_sqlCreateCommandProvider.GetSqlCreateCommandUtil(dataSource).AutoIncrementIdentityCommandGenerator()},\n");
                    continue;
                }
                queryString.Append($"{columnClause.ColumnName} {columnClause.ColumnDbType.GetDBType()} {collate} {nullOrNot},\n");
            }

            return queryString.ToString();
        }
    }
}
