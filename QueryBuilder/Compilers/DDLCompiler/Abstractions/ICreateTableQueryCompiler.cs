using System.Collections.Generic;
using System.Text;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface ICreateTableQueryCompiler
    {
        StringBuilder CompileCreateTable(string tableName,TableType tableType,List<CreateTableColumn> createTableColumnClauses);
    }
}
