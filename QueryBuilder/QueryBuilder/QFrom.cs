using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QFrom(QTableExpression Exp) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append("FROM ");
            Exp.Render(sb, r);
        }
    }
}