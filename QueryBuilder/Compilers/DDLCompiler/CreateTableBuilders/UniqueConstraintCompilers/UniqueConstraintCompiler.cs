using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlKata.Compilers.DDLCompiler.Abstractions;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.UniqueConstraintCompilers
{
    public class UniqueConstraintCompiler : IUniqueConstraintCompiler
    {
        public string CompileUniqueConstraints(List<CreateTableColumn> createTableColumnClauses)
        {
            var queryString = new StringBuilder();
            var uniqeColumns = createTableColumnClauses.Where(column => column.IsUnique).ToList();
            for (var i = 0; i < uniqeColumns.Count(); i++)
            {
                queryString.Append($"CONSTRAINT unique_constraint_{i} UNIQUE ({uniqeColumns[i].ColumnName}), \n");
            }
            return queryString.ToString();
        }
    }
}
