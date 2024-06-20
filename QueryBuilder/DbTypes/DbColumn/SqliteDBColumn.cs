using SqlKata.DbTypes.Enums;
using System.Text;

namespace SqlKata.DbTypes.DbColumn
{
    internal class SqliteDBColumn : BaseDBColumn
    {
        public SqliteDbType SqliteDbType { get; set; }  

        internal override string GetDBType()
        {
            var stringBuilder = new StringBuilder(SqliteDbType.ToString().Replace("_", " "));
            AddLengthAndPrecisionToType(stringBuilder);
            return stringBuilder.ToString();
        }
    }
}
