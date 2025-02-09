using System.Collections.Generic;
using System.Text;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface IUniqueConstraintCompiler
    {
        string CompileUniqueConstraints(List<CreateTableColumn> createTableColumnClauses);
    }
}
