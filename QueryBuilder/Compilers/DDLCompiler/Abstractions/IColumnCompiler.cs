using System.Collections.Generic;
using System.Text;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface IColumnCompiler
    {
        string CompileCreateTableColumns(List<CreateTableColumn> createTableColumnClauses);
    }
}
