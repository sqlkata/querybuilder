using SqlKata.DbTypes.Enums;
using System.Text;

namespace SqlKata.DbTypes.DbColumn
{
    public class SqlServerDBColumn : BaseDBColumn
    {
        public SqlServerDbType SqlServerDbType { get; set; }

        internal override string GetDBType()
        {
            var stringBuilder = new StringBuilder(SqlServerDbType.ToString().Replace("_", " "));
            AddLengthAndPrecisionToType(stringBuilder);
            return stringBuilder.ToString();
        }
    }
}
