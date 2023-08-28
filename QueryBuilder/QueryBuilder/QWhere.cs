using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QWhere(ImmutableArray<QConditionTag> Conditions) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append("WHERE ");
            sb.RenderList(" ", Conditions, r);
        }
    }
}