namespace SqlKata.Contract.CreateTable
{
    public class TableColumnDefenition
    {
        public string ColumnName { get; set; }
        public string ColumnDbType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsAutoIncrement { get; set; }
    }
}
