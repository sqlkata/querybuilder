using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QLastId : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(";");
            sb.Append(r.LastId);
        }
    }
}