using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
   
    public sealed record QInsertValues(
        ImmutableArray<QParameter> Values) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.RenderList(", ", Values, r);

        }
    }
}
