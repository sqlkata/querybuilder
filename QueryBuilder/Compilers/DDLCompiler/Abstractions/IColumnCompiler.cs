using System.Collections.Generic;
using System.Text;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface IColumnCompiler
    {
        void CompileCreateTableColumns(StringBuilder queryString, List<CreateTableColumn> createTableColumnClauses);
    }
}
