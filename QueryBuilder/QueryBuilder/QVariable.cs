using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QVariable(Variable Variable) : QParameter
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(r.ParameterPlaceholder);
        }
    }
}