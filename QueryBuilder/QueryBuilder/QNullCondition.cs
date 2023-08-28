using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QNullCondition(bool IsNot) : QCondition
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(IsNot ? "IS NOT NULL" : "IS NULL");
        }
    }
}