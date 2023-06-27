using SqlKata.Clauses;
using SqlKata.Contract.CreateTable;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {
        public void CreateTable(IEnumerable<TableColumnDefenitionDto> columns,TableType tableType = TableType.Permanent)
        {
            Method = "CreateTable";

            ClearComponent("TableType").AddComponent("TableType",new TableCluase()
            {
                TableType = tableType
            });

            columns.ToList().ForEach(column =>
            {
                AddComponent("CreateTableColumn",new CreateTableColumn()
                {
                    ColumnName = column.ColumnName,
                    ColumnDbType = column.ColumnDbType,
                    IsNullable = column.IsNullable,
                    IsUnique = column.IsUnique,
                    IsPrimaryKey = column.IsPrimaryKey,
                    IsAutoIncrement = column.IsAutoIncrement,
                    IsIdentity = column.IsIdentity
                });
            });
        }

    }
}
