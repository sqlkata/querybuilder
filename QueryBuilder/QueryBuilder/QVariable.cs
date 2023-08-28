using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QVariable(Variable Variable) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(r.ParameterPlaceholder);
        }
    }
}
