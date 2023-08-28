using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public record QNot(bool IsNot, Q Condition) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            if (IsNot)
            {
                sb.Append("NOT (");
            }
            Condition.Render(sb, r);
            if (IsNot)
            {
                sb.Append(")");
            }
        }
    }
}