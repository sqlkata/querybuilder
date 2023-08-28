using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public record QHeader(string Header, Q Expression) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(Header);
            sb.Append(" ");
            Expression.Render(sb, r);
        }
    }
}