using SqlKata.SqlQuery.DDL.Dtos.CreateTable;
using System.Collections.Generic;

namespace SqlKata
{
    public partial class Query
    {
        public Query CreateTable(string tableName, IEnumerable<TableColumnDefenition> columns,bool isTempTable)
        {
            Method = "CreateTable";



            return null;
        }

    }
}
