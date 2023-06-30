using SqlKata.DbTypes.Enums;
using System.Text;

namespace SqlKata.DbTypes.DbColumn
{
    public class PostgresqlDBColumn : BaseDBColumn
    {
        public PostgresqlDbType PostgresqlDbType { get; set; }

        internal override string GetDBType()
        {
            var stringBuilder = new StringBuilder(PostgresqlDbType.ToString());
            AddLengthAndPrecisionToType(stringBuilder);
            return stringBuilder.ToString();
        }
    }
}
