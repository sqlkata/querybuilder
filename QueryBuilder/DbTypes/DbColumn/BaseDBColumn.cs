using System.Text;

namespace SqlKata.DbTypes.DbColumn
{
    public abstract class BaseDBColumn
    {
        public int? Length { get; set; } 
        public int? Precision { get; set; }
        internal abstract string GetDBType();

        protected void AddLengthAndPrecisionToType(StringBuilder stringBuilder)
        {
            if (Length.HasValue || Precision.HasValue)
            {
                stringBuilder.Append('(');
                if (Length.HasValue)
                {
                    stringBuilder.Append(Length.Value.ToString());
                }
                if (Precision.HasValue)
                {
                    stringBuilder.Append(",");
                    stringBuilder.Append(Precision.Value.ToString());
                }
                stringBuilder.Append(")");
            }
        }
    }
}
