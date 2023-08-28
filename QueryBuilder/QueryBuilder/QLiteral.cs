using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QLiteral(string Literal) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(Literal);
        }
    }
}
