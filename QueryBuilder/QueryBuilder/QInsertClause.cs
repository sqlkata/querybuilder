using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QInsertClause(
        ImmutableArray<string> Columns,
        ImmutableArray<QParameter> Values) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            if (Columns.Length == 0) return;
            sb.Append(" (");
            sb.RenderList(", ", Columns, c => r.X.Wrap(sb, c));
            sb.Append(")");
            sb.Append(" VALUES (");
            sb.RenderList(", ", Values, r);
            sb.Append(")");

        }
    }
}