using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public record QPrefix(string Header, Q Expression) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(Header);
            Expression.Render(sb, r);
        }
    }
}