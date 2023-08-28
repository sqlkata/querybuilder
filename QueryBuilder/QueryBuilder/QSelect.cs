using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QSelect(bool IsDistinct, ImmutableArray<QColumn> Columns) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append("SELECT ");
            if (IsDistinct)
            {
                sb.Append("DISTINCT ");
            }
            if (Columns.IsEmpty)
            {
                sb.Append("*");
                return;
            }
            sb.RenderList(", ", Columns, r);
            //return $"SELECT {distinct}{select}";
        }
    }
}