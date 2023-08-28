using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QRoundBraces(Q Exp) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append("(");
            Exp.Render(sb, r);
            sb.Append(")");
        }
    }
}