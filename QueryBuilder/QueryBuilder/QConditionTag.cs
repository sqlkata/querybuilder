using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public record QConditionTag(bool? IsOr, Q Condition) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            if (IsOr is true)
            {
                sb.Append("OR ");
            }
            else if (IsOr is false)
            {
                sb.Append("AND ");
            }
            Condition.Render(sb, r);
        }
    }
}