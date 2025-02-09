using System.Collections.Generic;
using System.Text;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface IColumnCompiler
    {
        string CompileCreateTableColumns(List<CreateTableColumn> createTableColumnClauses,DataSource dataSource);
    }
}
