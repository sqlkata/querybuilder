using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QInsertColumns(ImmutableArray<string> Names) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append("(");
            sb.RenderList(", ", Names, c => r.X.Wrap(sb, c));
            sb.Append(")");
        }
    }
}
