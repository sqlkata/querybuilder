using SqlKata.DbTypes.Enums;
using System.Text;

namespace SqlKata.DbTypes.DbColumn
{
    public class MySqlDBColumn : BaseDBColumn
    {
        public MySqlDbType MySqlDbType { get; set; }

        internal override string GetDBType()
        {
            var stringBuilder = new StringBuilder(MySqlDbType.ToString().Replace("_"," "));
            AddLengthAndPrecisionToType(stringBuilder);
            return stringBuilder.ToString();
        }
    }
}
