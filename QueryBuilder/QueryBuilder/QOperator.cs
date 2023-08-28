using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QOperator(string Operator) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(r.Operators.CheckOperator(Operator));
        }
    }
}