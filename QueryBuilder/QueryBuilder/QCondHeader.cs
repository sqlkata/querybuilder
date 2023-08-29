using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public record QCondSandwich(bool Show, string Header, Q Expression, string Footer) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            if (Show)
            {
                sb.Append(Header);
                sb.Append(" ");
            }

            Expression.Render(sb, r);
            if (Show)
            {
                sb.Append(" ");
                sb.Append(Footer);
            }

        }
    }

    public record QCondHeader(bool Show, string Header, Q Expression) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            if (Show)
            {
                sb.Append(Header);
                sb.Append(" ");
            }

            Expression.Render(sb, r);
        }
    }
}
