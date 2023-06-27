using SqlKata.Contract.CreateTable;
using System.Collections.Generic;

namespace SqlKata
{
    public partial class Query
    {
        public void CreateTable(IEnumerable<TableColumnDefenition> columns,bool isTempTable)
        {
            Method = "CreateTable";


        }

    }
}
