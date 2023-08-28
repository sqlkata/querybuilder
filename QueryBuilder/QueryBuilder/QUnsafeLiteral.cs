using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QUnsafeLiteral(UnsafeLiteral Literal) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(Literal.Value);
        }
    }
}
