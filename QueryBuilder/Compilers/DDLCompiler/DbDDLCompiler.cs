using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Contract.CreateTable;
using SqlKata.Exceptions.CreateTableQuery;
using System.Linq;
using System.Text;
using System.Xml;

namespace SqlKata.Compilers.DDLCompiler
{
    internal class DbDDLCompiler
    {
        private readonly ISqlCreateCommandUtil _sqlCommandUtil;
        internal DbDDLCompiler(ISqlCreateCommandUtil sqlCreateCommandUtil)
        {
            _sqlCommandUtil = sqlCreateCommandUtil;
        }

        internal SqlResult CompileCreateTable(Query query)
        {
            var result = new SqlResult()
            {
                Query = query.Clone(),
            };
            var tableName = result.Query.GetOneComponent<FromClause>("from").Table;
            var tableType = result.Query.GetOneComponent<TableCluase>("TableType").TableType;
            var queryString = new StringBuilder(_sqlCommandUtil.CreateTableCommandGenerator(tableType,tableName));
            queryString.Append("(\n");
            var createTableColumnCluases = result.Query.GetComponents<CreateTableColumn>("CreateTableColumn");

            var identityAndAutoIncrementColumns = createTableColumnCluases.Where(x => x.IsIdentity || x.IsAutoIncrement);
            if (identityAndAutoIncrementColumns.Count() > 1)
            {
                throw new AutoIncrementOrIdentityExceededException("table can not have more than one auto increment or identity column");
            }
            foreach (var columnCluase in createTableColumnCluases)
            {
                var nullOrNot = columnCluase.IsNullable ? "NULL " : "NOT NULL ";
                if (columnCluase.IsIdentity || columnCluase.IsAutoIncrement)
                {
                    queryString.Append($"{columnCluase.ColumnName} {columnCluase.ColumnDbType.GetDBType()}  {_sqlCommandUtil.AutoIncrementIdentityCommandGenerator()},\n");
                    continue;
                }
                queryString.Append($"{columnCluase.ColumnName} {columnCluase.ColumnDbType.GetDBType()} {nullOrNot},\n");
            }
            var primaryKeys = createTableColumnCluases.Where(column => column.IsPrimaryKey);
            if (primaryKeys.Any())
                queryString.Append(string.Format("PRIMARY KEY ({0}),\n", string.Join(",", primaryKeys.Select(column => column.ColumnName))));

            var uniqeColumns = createTableColumnCluases.Where(column => column.IsUnique).ToList();
            for (var i = 0; i < uniqeColumns.Count(); i++)
            {
                queryString.Append($"CONSTRAINT unique_constraint_{i} UNIQUE ({uniqeColumns[i].ColumnName}), \n");
            }
            queryString.Append(")\n");
            result.RawSql = RefineQueryString(queryString.ToString());
            result.RawSql.LastIndexOf(",");
            return result;
        }

        private string RefineQueryString(string queryString)
        {
            var lastCommaChar = queryString.LastIndexOf(',');
            if(lastCommaChar != -1)
                queryString = queryString.Remove(lastCommaChar,1);
            return queryString;
        }


    }
}
