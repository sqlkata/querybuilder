using SqlKata.DbTypes.Enums;
using System.Text;

namespace SqlKata.DbTypes.DbColumn
{
    public class OracleDBColumn : BaseDBColumn
    {
        public OracleDbType OracleDbType { get; set; }

        internal override string GetDBType()
        {
            var stringBuilder = new StringBuilder(OracleDbType.ToString());
            AddLengthAndPrecisionToType(stringBuilder);
            return stringBuilder.ToString();
        }
    }
}
