using System.Collections.Generic;
using System.Text;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface IPrimaryKeyCompiler
    {
        void CompilePrimaryKey(StringBuilder queryString, List<CreateTableColumn> createTableColumnClauses);
    }
}
