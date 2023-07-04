using SqlKata.DbTypes.DbColumn;

namespace SqlKata.Contract.CreateTable
{
    public class TableColumnDefinitionDto
    {
        public string ColumnName { get; set; }
        public BaseDBColumn ColumnDbType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsAutoIncrement { get; set; }
        public string Collate { get; set; }
    }
}
