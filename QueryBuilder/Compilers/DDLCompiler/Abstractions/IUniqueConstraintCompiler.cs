using System.Collections.Generic;
using System.Text;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    public interface IUniqueConstraintCompiler
    {
        void CompileUniqueConstraints(StringBuilder queryString, List<CreateTableColumn> createTableColumnClauses);
    }
}
