using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlKata.Compilers.DDLCompiler.Abstractions;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.PrimaryKeyCompilers
{
    internal class PrimaryKeyCompiler : IPrimaryKeyCompiler
    {
        public string CompilePrimaryKey(List<CreateTableColumn> createTableColumnClauses)
        {
            var queryString = new StringBuilder();
            var primaryKeys = createTableColumnClauses.Where(column => column.IsPrimaryKey);
            if (primaryKeys.Any())
                queryString.Append(string.Format("PRIMARY KEY ({0}),\n", string.Join(",", primaryKeys.Select(column => column.ColumnName))));
            return queryString.ToString();
        }
    }
}
