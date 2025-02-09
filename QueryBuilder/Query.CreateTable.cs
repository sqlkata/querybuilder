using SqlKata.Clauses;
using SqlKata.Contract.CreateTable;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Contract.CreateTable.DbTableSpecific;
using SqlKata.Exceptions.CreateTableQuery;

namespace SqlKata
{
    public partial class Query
    {
        public Query CreateTable(IEnumerable<TableColumnDefinitionDto> columns,TableType tableType = TableType.Permanent,CreateDbTableExtension createDbTableExtension = null)
        {
            Method = "CreateTable";

            AddTableTypeComponent(tableType);

            AddDbSpecificCreateTableComponent(createDbTableExtension);

            columns.ToList().ForEach(column =>
            {
                AddComponent("CreateTableColumn",new CreateTableColumn()
                {
                    Component = "CreateTableColumn",
                    ColumnName = column.ColumnName,
                    ColumnDbType = column.ColumnDbType,
                    IsNullable = column.IsNullable,
                    IsUnique = column.IsUnique,
                    IsPrimaryKey = column.IsPrimaryKey,
                    IsAutoIncrement = column.IsAutoIncrement,
                    IsIdentity = column.IsIdentity,
                    Collate = column.ColumnDbType.Collation
                });
            });

            return this;
        }

        public Query CreateTableAs(Query selectQuery, TableType tableType = TableType.Permanent,CreateDbTableExtension createDbTableExtension = null)
        {
            Method = "CreateTableAs";
            if (selectQuery.Method != "select")
            {
                throw new InvalidQueryMethodException("Inner query of CREATE TABLE AS must be select query");
            }

            AddDbSpecificCreateTableComponent(createDbTableExtension);
            AddTableTypeComponent(tableType);

            AddComponent("CreateTableAsQuery", new CreateTableAsClause
            {
                SelectQuery = selectQuery,
                Component = "CreateTableAsQuery"
            });

            return this;
        }

        private void AddDbSpecificCreateTableComponent(CreateDbTableExtension createDbTableExtension)
        {
            if (createDbTableExtension != null)
            {
                AddComponent("CreateTableExtension", new CreateTableQueryExtensionClause()
                {
                    CreateDbTableExtension = createDbTableExtension,
                    Component = "CreateTableExtension"
                });
            }
        }
        private void AddTableTypeComponent(TableType tableType)
        {
            ClearComponent("TableType").AddComponent("TableType", new TableCluase()
            {
                TableType = tableType,
                Component = "TableType"
            });
        }


    }
}
