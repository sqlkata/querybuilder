using System.Collections.Generic;
using System.Text;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface IPrimaryKeyCompiler
    {
        string CompilePrimaryKey(List<CreateTableColumn> createTableColumnClauses);
    }
}
